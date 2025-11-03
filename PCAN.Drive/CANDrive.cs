using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.Drive.Modle;
using PCAN.Notification.Log;
using Peak.Can.Basic;
using System.Diagnostics;
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
        private PcanChannel PcanHandle;
        private uint m_DeviceID;
        private Bitrate m_Baudrate;
        private readonly IMediator _mediator;
     
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
        public CANDrive(ushort handle,uint driveid, Bitrate Baudrate,IMediator mediator,int sleeptime)
        {
            PcanHandle = (PcanChannel)handle;
            m_DeviceID = driveid;
            m_Baudrate = Baudrate;
            _mediator = mediator;
            m_SleepTime = sleeptime;
            CANReadMsgSubject = new Subject<ReadMessage>();
            CANReadMsg = CANReadMsgSubject.AsObservable();
            var status = CANInit();
            if (status == PcanStatus.OK)
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
                    
                    PcanMessage msg = new PcanMessage();
                    msg.ID = (uint)writemsg.Id;
                    msg.DLC = (byte)writemsg.Data.Length;
                    msg.Data = writemsg.Data;
                    msg.MsgType = writemsg.MessageType;
                    //if (msg.DATA.Length<8)
                    //{
                    //    Array.Resize(ref msg.DATA, 8);
                    //}
                    var result = Write(msg);
                    if (result != PcanStatus.OK)
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
        public CANDrive(ushort handle, uint driveid, string Baudrate, IMediator mediator, int sleeptime,bool useFD)
        {
            PcanHandle = (PcanChannel)handle;
            m_DeviceID = driveid;
            UseFD=useFD;
            _mediator = mediator;
            m_SleepTime = sleeptime;
            CANReadMsgSubject = new Subject<ReadMessage>();
            CANReadMsg = CANReadMsgSubject.AsObservable();
            var status = CANInitFD(Baudrate);
            if (status == PcanStatus.OK)
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

                    PcanMessage msg = new((uint)writemsg.Id, writemsg.MessageType, (byte)writemsg.Data.Length, writemsg.Data,extendedDataLength:true);
                    //if (msg.Data < 64)
                    //{
                    //    Array.Resize(ref msg.DATA, 64);
                    //}
                    var result = WriteFD(msg);
                    if (result != PcanStatus.OK)
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
        private PcanStatus ReadMessage()
        {
            try
            {
                PcanMessage _CANMsg;
                ulong CANTimeStamp;
                ushort length;
                var stsResult = Api.Read(PcanHandle, out _CANMsg, out CANTimeStamp);
                if (stsResult == PcanStatus.OK)
                {
                    var message = new ReadMessage()
                    {
                        ID = (int)_CANMsg.ID,
                        LEN = _CANMsg.Length,
                        MSGTYPE = _CANMsg.MsgType,
                        DATA = _CANMsg.Data,
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
                return PcanStatus.Unknown;
            }
           
        }
        private void ReadMessages()
        {
            ReadMessage();
            
        }
        private PcanStatus ReadMessageFD()
        {
            try
            {
                PcanMessage _CANMsg;
                ulong CANTimeStamp;
                ushort length;
                
                var stsResult = Api.Read(PcanHandle, out _CANMsg, out CANTimeStamp);
                if (stsResult == PcanStatus.OK)
                {
                    var len = _CANMsg.DLC > 8 ? (byte)(8 + (_CANMsg.DLC - 8) * 4) : _CANMsg.DLC;
                    byte[] datas = _CANMsg.Data;
                    var message = new ReadMessage()
                    {
                        ID = (int)_CANMsg.ID,
                        LEN = len,
                        MSGTYPE = _CANMsg.MsgType,
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
                return PcanStatus.Unknown;
            }

        }
        private void ReadMessagesFD()
        {
            ReadMessageFD();

        }

        #endregion

        #region Write
        private PcanStatus Write(PcanMessage msg)
        {
            try
            {
                return Api.Write(PcanHandle,  msg);

            }
            catch (Exception ex)
            {

                _mediator.Publish(new LogNotification
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.CanDevice,
                    Message = $"写入时出现系统错误：{ex.Message}"
                });
                return PcanStatus.Unknown;
            }
        }
        private PcanStatus WriteFD(PcanMessage msg)
        {
            try
            {
                return Api.Write(PcanHandle,  msg);

            }
            catch (Exception ex)
            {

                _mediator.Publish(new LogNotification
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.CanDevice,
                    Message = $"写入时出现系统错误：{ex.Message}"
                });
                return PcanStatus.Unknown;
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
        private PcanStatus CANInit()
        {
           return Api.Initialize(PcanHandle, m_Baudrate);
        }
        private PcanStatus CANInitFD(string bitrateFD)
        {
            var a= Api.Initialize(PcanHandle,new BitrateFD( bitrateFD));
            return a;
        }
        public void CLose()
        {
            _tokensource.Cancel();
            Api.Uninitialize(PcanHandle);
            this.IsReadly = false;

        }
        public bool FilterMessages(uint fromid,uint toid)
        {
            var status= Api.FilterMessages(PcanHandle, fromid, toid, UseFD ? FilterMode.Extended: FilterMode.Standard);
            if (status == PcanStatus.OK)
            {
                return true;
            }
            else
            {
                _mediator.Publish(new LogNotification() { LogLevel= LogLevel.Error, LogSource = LogSource.CanDevice, Message = $"设置过滤器失败:{status}" });
                return false;
            }
        }

    }
}
