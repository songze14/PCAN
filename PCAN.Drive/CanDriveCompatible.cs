using DynamicData.Aggregation;
using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.Drive.Modle;
using PCAN.Notification.Log;
using PCAN.Shard.Tools;
using Peak.Can.Basic1;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using TPCANHandle = System.UInt16;


namespace PCAN.Drive
{
    /// <summary>
    /// PCAN驱动兼容类
    /// </summary>
    public class CanDriveCompatible
    {

        private TPCANHandle PcanHandle;
        private uint m_DeviceID;
        private TPCANBaudrate m_Baudrate;
        private readonly IMediator _mediator;

        private int m_SleepTime;
        private CancellationTokenSource _tokensource = new CancellationTokenSource();
        private Subject<PCanWriteMessage> CanWriteMessages { get; set; } = new Subject<PCanWriteMessage>();
        /// <summary>
        /// 读取的信息
        /// </summary>
        private Subject<ReadMessage> CANReadMsgSubject { get; set; }
        public new IObservable<ReadMessage> CANReadMsg { get; set; }
        public new bool IsReadly { get; set; } = false;
        private bool UseFD { get; set; }
        /// <summary>
        /// 构造 父类的构造参无所谓，并不使用父类
        /// </summary>
        /// <param name="driveid">设备ID</param>
        /// <param name="Baudrate">波特率</param>
        /// <param name="mediator">消息中转</param>
        public CanDriveCompatible(ushort handle, uint driveid, TPCANBaudrate Baudrate, IMediator mediator, int sleeptime)
        {
            PcanHandle = (TPCANHandle)handle;
            m_DeviceID = driveid;
            m_Baudrate = Baudrate;
            _mediator = mediator;
            m_SleepTime = sleeptime;
            CANReadMsgSubject = new Subject<ReadMessage>();
            CANReadMsg = CANReadMsgSubject.AsObservable();
            var status = CANInit();
            if (status == TPCANStatus.PCAN_ERROR_OK)
            {
                this.IsReadly = true;
                var token = _tokensource.Token;
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        ReadMessages();
                    }
                }, token);
            }
            else
            {
               
                mediator.Publish(new LogNotification
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.CanDevice,
                    Message = $"CAN初始化失败，请检查参数:{status}"
                });
            }
            ;

            this.CanWriteMessages.AsObservable().Subscribe(writemsg =>
            {
                try
                {
                    ///CAN协议规定数据长度为8位
                    var senddata = new byte[8];
                    Array.Copy(writemsg.Data, senddata, writemsg.Data.Length);
                    TPCANMsg msg = new TPCANMsg();
                    msg.ID = (uint)writemsg.Id;
                    msg.LEN = (byte)8;
                    msg.DATA=writemsg.Data;
                    msg.MSGTYPE =(TPCANMessageType) writemsg.MessageType;
                    //if (msg.DATA.Length<8)
                    //{
                    //    Array.Resize(ref msg.DATA, 8);
                    //}
                    var result = Write(msg);
                    if (result !=TPCANStatus.PCAN_ERROR_OK)
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
        public CanDriveCompatible(ushort handle, uint driveid, string Baudrate, IMediator mediator, int sleeptime, bool useFD) 
        {
            PcanHandle = (TPCANHandle)handle;
            m_DeviceID = driveid;
            UseFD = useFD;
            _mediator = mediator;
            m_SleepTime = sleeptime;
            CANReadMsgSubject = new Subject<ReadMessage>();
            CANReadMsg = CANReadMsgSubject.AsObservable();
            var status = CANInitFD(Baudrate);
            if (status == TPCANStatus.PCAN_ERROR_OK)
            {
                this.IsReadly = true;
                var token = _tokensource.Token;
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        ReadMessagesFD();
                        //await Task.Delay(m_SleepTime);
                    }
                }, token);

            }
            else
            {
                mediator.Publish(new LogNotification
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.CanDevice,
                    Message = $"CANFD初始化失败，请检查参数:{status}"
                });
            }
            ;
            this.CanWriteMessages.AsObservable().Subscribe(writemsg =>
            {
                try
                {
                    ///CANFD协议规定数据长度为1-8，12，16，24，32位
                    var datalength = MathTool.GetDLCFromLength(writemsg.Data.Length);
                    TPCANMsgFD msg =  new TPCANMsgFD();
                    msg.ID = (uint)writemsg.Id;
                    msg.DLC = (byte)datalength;
                    msg.DATA = writemsg.Data;
                    msg.MSGTYPE = (TPCANMessageType)writemsg.MessageType;
                    //if (msg.Data < 64)
                    //{
                    //    Array.Resize(ref msg.DATA, 64);
                    //}
                    var result = WriteFD(msg);
                    if (result != TPCANStatus.PCAN_ERROR_OK)
                    {
                        StringBuilder errtext = new();
                        var errtextstatus = PCANBasicCompatible.GetErrorText(result, 0, errtext);
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
                var stsResult = PCANBasicCompatible.Read(PcanHandle, out _CANMsg, out CANTimeStamp);
                if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                {
                    var message = new ReadMessage()
                    {
                        ID = (int)_CANMsg.ID,
                        LEN = _CANMsg.LEN,
                        MSGTYPE = (Peak.Can.Basic.MessageType)_CANMsg.MSGTYPE,
                        DATA = _CANMsg.DATA,
                        TimeStamp = CANTimeStamp.millis_overflow * 65536.0 
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

                var stsResult = PCANBasicCompatible.ReadFD(PcanHandle, out _CANMsg, out CANTimeStamp);
                if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                {
                    var len = _CANMsg.DLC > 8 ? (byte)(8 + (_CANMsg.DLC - 8) * 4) : _CANMsg.DLC;
                    byte[] datas = _CANMsg.DATA;
                    var message = new ReadMessage()
                    {
                        ID = (int)_CANMsg.ID,
                        LEN = len,
                        MSGTYPE = (Peak.Can.Basic.MessageType)_CANMsg.MSGTYPE,
                        DATA = datas[0..len],
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
                return PCANBasicCompatible.Write(PcanHandle,ref msg);

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
                return PCANBasicCompatible.WriteFD(PcanHandle,ref msg);

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

        public new int ResendCount;
        private TPCANStatus CANInit()
        {
            return PCANBasicCompatible.Initialize(PcanHandle, m_Baudrate);
        }
        private TPCANStatus CANInitFD(string bitrateFD)
        {
            var a = PCANBasicCompatible.InitializeFD(PcanHandle, bitrateFD);
            return a;
        }
        public new void CLose()
        {
            _tokensource.Cancel();
            PCANBasicCompatible.Uninitialize(PcanHandle);
            this.IsReadly = false;
        }
        public new bool Reset()
        {
            var status = PCANBasicCompatible.Reset(PcanHandle);
            if (status == TPCANStatus.PCAN_ERROR_OK)
            {
                return true;
            }
            else
            {
                _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.CanDevice, Message = $"重置管道失败：{status}" });
                return false;
            }
        }
        public new bool FilterMessages(uint fromid, uint toid)
        {
            var status = PCANBasicCompatible.FilterMessages(PcanHandle, fromid, toid, UseFD ? TPCANMode.PCAN_MODE_EXTENDED : TPCANMode.PCAN_MODE_STANDARD);
            if (status == TPCANStatus.PCAN_ERROR_OK)
            {
                return true;
            }
            else
            {
                _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.CanDevice, Message = $"设置过滤器失败:{status}" });
                return false;
            }
        }

    }
}
