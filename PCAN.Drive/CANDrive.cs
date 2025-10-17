using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.Drive.Modle;
using PCAN.Notification.Log;
using Peak.Can.Basic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TPCANHandle = System.UInt16;

namespace PCAN.Drive
{
    /// <summary>
    /// CAN通信驱动
    /// </summary>
    public class CANDrive 
    {
        private TPCANHandle PcanHandle;
        private uint m_DeviceID;
        private TPCANBaudrate m_Baudrate;
        private readonly IMediator _mediator;
        private TPCANType m_HwType;
        private int m_SleepTime;
        private CancellationTokenSource _tokensource=new CancellationTokenSource();
        private Subject<PCanWriteMessage> CanWriteMessages { get; set; } = new Subject<PCanWriteMessage>();
        /// <summary>
        /// 读取的信息
        /// </summary>
        private Subject<ReadMessage> CANReadMsgSubject { get; set; }
        public IObservable<ReadMessage> CANReadMsg { get; set; }
        public bool IsReadly { get; set; } = false;
        private bool UseFD { get; set; }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="driveid">设备ID</param>
        /// <param name="Baudrate">波特率</param>
        /// <param name="mediator">消息中转</param>
        public CANDrive(TPCANHandle handle,uint driveid, TPCANBaudrate Baudrate,IMediator mediator,int sleeptime)
        {
            PcanHandle = handle;
            m_DeviceID = driveid;
            m_Baudrate = Baudrate;
            _mediator = mediator;
            m_SleepTime = sleeptime;
            CANReadMsgSubject = new Subject<ReadMessage>();
            CANReadMsg = CANReadMsgSubject.AsObservable();
            if (CANInit()== TPCANStatus.PCAN_ERROR_OK)
            {
                this.IsReadly = true;
                var token = _tokensource.Token;
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        ReadMessages();
                        await Task.Delay(m_SleepTime);
                    }
                }, token);
                //Task.Run(async () =>
                //{
                //    while (!token.IsCancellationRequested)
                //    {
                //        WriteMessages();
                //        await Task.Delay(m_SleepTime);

                //    }
                //}, token);
            } ;
            this.CanWriteMessages.AsObservable().Subscribe(writemsg =>
            {
                try
                {
                    
                    TPCANMsg msg = new TPCANMsg();
                    msg.ID = (uint)writemsg.Id;
                    msg.LEN = (byte)writemsg.Data.Length;
                    msg.DATA = writemsg.Data;
                    msg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                    if (msg.DATA.Length<8)
                    {
                        Array.Resize(ref msg.DATA, 8);
                    }
                    var result = Write(msg);
                    if (result != TPCANStatus.PCAN_ERROR_OK)
                    {
                        _mediator.Publish(new LogNotification
                        {
                            LogLevel = LogLevel.Error,
                            LogSource = LogSource.CanDevice,
                            Message = $"写入时出现错误：信息状态{result},重试ID{writemsg.Id}"
                        });
                        ResendCount++;
                        if (ResendCount >= 10)
                        {
                            _mediator.Publish(new LogNotification
                            {
                                LogLevel = LogLevel.Error,
                                LogSource = LogSource.CanDevice,
                                Message = $"写入时出现错误：已重试10次，取消发送！"
                            });
                            return;
                        }
                        CanWriteMessages.OnNext(writemsg);
                    }
                    ResendCount = 0;

                }
                catch (Exception ex)
                {
                    _mediator.Publish(new LogNotification
                    {
                        LogLevel = LogLevel.Error,
                        LogSource = LogSource.CanDevice,
                        Message = $"写入时出现系统错误：{ex.Message}"
                    });
                }
            });
        }
        public CANDrive(TPCANHandle handle, uint driveid, string Baudrate, IMediator mediator, int sleeptime,bool useFD)
        {
            PcanHandle = handle;
            m_DeviceID = driveid;
     
            _mediator = mediator;
            m_SleepTime = sleeptime;
            CANReadMsgSubject = new Subject<ReadMessage>();
            CANReadMsg = CANReadMsgSubject.AsObservable();
            if (CANInitFD(Baudrate) == TPCANStatus.PCAN_ERROR_OK)
            {
                this.IsReadly = true;
                var token = _tokensource.Token;
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        ReadMessagesFD();
                        await Task.Delay(m_SleepTime);
                    }
                }, token);
               
            }
            ;
            this.CanWriteMessages.AsObservable().Subscribe(writemsg =>
            {
                try
                {

                    TPCANMsgFD msg = new TPCANMsgFD();
                    msg.ID = (uint)writemsg.Id;
                    msg.DLC = (byte)writemsg.Data.Length;
                    msg.DATA = writemsg.Data;
                    msg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                    if (msg.DATA.Length < 64)
                    {
                        Array.Resize(ref msg.DATA, 64);
                    }
                    var result = WriteFD(msg);
                    if (result != TPCANStatus.PCAN_ERROR_OK)
                    {
                        _mediator.Publish(new LogNotification
                        {
                            LogLevel = LogLevel.Error,
                            LogSource = LogSource.CanDevice,
                            Message = $"写入时出现错误：信息状态{result},重试ID{writemsg.Id}"
                        });
                        ResendCount++;
                        if (ResendCount >= 10)
                        {
                            _mediator.Publish(new LogNotification
                            {
                                LogLevel = LogLevel.Error,
                                LogSource = LogSource.CanDevice,
                                Message = $"写入时出现错误：已重试10次，取消发送！"
                            });
                            return;
                        }
                       Thread.Sleep(m_SleepTime);
                        CanWriteMessages.OnNext(writemsg);
                    }
                    ResendCount = 0;

                }
                catch (Exception ex)
                {
                    _mediator.Publish(new LogNotification
                    {
                        LogLevel = LogLevel.Error,
                        LogSource = LogSource.CanDevice,
                        Message = $"写入时出现系统错误：{ex.Message}"
                    });
                }
            });
        }
        #region Read
        private TPCANStatus ReadMessage()
        {
            try
            {
                TPCANMsg _CANMsg;
                TPCANTimestamp CANTimeStamp;
                ushort length;
                var stsResult = PCANBasic.Read(PcanHandle, out _CANMsg, out CANTimeStamp);
                if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                {
                    var message = new ReadMessage()
                    {
                        ID = (int)_CANMsg.ID,
                        LEN = _CANMsg.LEN,
                        MSGTYPE = _CANMsg.MSGTYPE,
                        DATA = _CANMsg.DATA,
                        TimeStamp = CANTimeStamp.millis + CANTimeStamp.micros / 1000.0
                    };

                    CANReadMsgSubject.OnNext(message);
                }
                return stsResult;
            }
            catch (Exception ex)
            {
               _mediator.Publish(new LogNotification
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.CanDevice,
                    Message = $"读取时出现系统错误：{ex.Message}"
                });
                return TPCANStatus.PCAN_ERROR_UNKNOWN;
            }
           
        }
        private void ReadMessages()
        {
            ReadMessage();
            
        }
        private TPCANStatus ReadMessageFD()
        {
            try
            {
                TPCANMsgFD _CANMsg;
                ulong CANTimeStamp;
                ushort length;
                var stsResult = PCANBasic.ReadFD(PcanHandle, out _CANMsg, out CANTimeStamp);
                if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                {
                    var message = new ReadMessage()
                    {
                        ID = (int)_CANMsg.ID,
                        LEN = _CANMsg.DLC>8?(byte)(8+(_CANMsg.DLC-8)*4):_CANMsg.DLC,
                        MSGTYPE = _CANMsg.MSGTYPE,
                        DATA = _CANMsg.DATA[0..(_CANMsg.DLC > 8 ? (byte)(8 + (_CANMsg.DLC - 8) * 4) : _CANMsg.DLC)],
                        TimeStamp = CANTimeStamp / 1000.0
                    };

                    CANReadMsgSubject.OnNext(message);
                }
                return stsResult;
            }
            catch (Exception ex)
            {
                _mediator.Publish(new LogNotification
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.CanDevice,
                    Message = $"读取时出现系统错误：{ex.Message}"
                });
                return TPCANStatus.PCAN_ERROR_UNKNOWN;
            }

        }
        private void ReadMessagesFD()
        {
            ReadMessageFD();

        }

        #endregion

        #region Write
        private TPCANStatus Write(TPCANMsg msg)
        {
            try
            {
                return PCANBasic.Write(PcanHandle, ref msg);

            }
            catch (Exception ex)
            {

                _mediator.Publish(new LogNotification
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.CanDevice,
                    Message = $"写入时出现系统错误：{ex.Message}"
                });
                return TPCANStatus.PCAN_ERROR_UNKNOWN;
            }
        }
        private TPCANStatus WriteFD(TPCANMsgFD msg)
        {
            try
            {
                return PCANBasic.WriteFD(PcanHandle, ref msg);

            }
            catch (Exception ex)
            {

                _mediator.Publish(new LogNotification
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.CanDevice,
                    Message = $"写入时出现系统错误：{ex.Message}"
                });
                return TPCANStatus.PCAN_ERROR_UNKNOWN;
            }
        }

        public void AddMessage(PCanWriteMessage message)
        {
            if (message.Data.Length < 8)
            {
                //var data = message.Data;
                //var newdata = new byte[8];
                //Array.Copy(data, newdata, data.Length);
                //message.Data = newdata;
            }
            CanWriteMessages.OnNext(message);
        }
        #endregion

        public int ResendCount;
        private TPCANStatus CANInit()
        {
           return PCANBasic.Initialize(PcanHandle, m_Baudrate);
        }
        private TPCANStatus CANInitFD(string bitrateFD)
        {
            var a= PCANBasic.InitializeFD(PcanHandle, bitrateFD);
            return a;
        }
        public void CLose()
        {
            _tokensource.Cancel();
            PCANBasic.Uninitialize(PcanHandle);
            this.IsReadly = false;

        }


    }
}
