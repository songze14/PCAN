using MediatR;
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
        private List<PCanWriteMessage> CanMessages { get; set; } = new List<PCanWriteMessage>();
        /// <summary>
        /// 读取的信息
        /// </summary>
        private Subject<ReadMessage> CANMsgSubject { get; set; }
        public IObservable<ReadMessage> CANMsg { get; set; }
        public bool IsReadly { get; set; } = false;
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
            CANMsgSubject = new Subject<ReadMessage>();
            CANMsg = CANMsgSubject.AsObservable();
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
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        WriteMessages();
                        await Task.Delay(m_SleepTime);

                    }
                }, token);
            } ;
            
        }
        #region Read
        private TPCANStatus ReadMessage()
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
                    DATA =BitConverter.ToString( _CANMsg.DATA),
                    TimeStamp = CANTimeStamp.millis + CANTimeStamp.micros / 1000.0
                };
                CANMsgSubject.OnNext(message);
            }
            return stsResult;
        }
        private void ReadMessages()
        {
            ReadMessage();
            
        }
        #endregion

        #region Write
        private TPCANStatus Write(TPCANMsg msg)
        {
           return PCANBasic.Write(PcanHandle, ref msg);
        }

        private void WriteMessages()
        {
            while (CanMessages.Count > 0)
            {
                try
                {
                    var item = CanMessages[0];
                    TPCANMsg msg = new TPCANMsg();
                    msg.ID = (uint)item.Id;
                    msg.LEN = (byte)item.Data.Length;
                    msg.DATA = item.Data;
                    msg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                    var result = Write(msg);
                    if (result != TPCANStatus.PCAN_ERROR_OK)
                    {
                        _mediator.Publish(new LogNotification
                        {
                            LogSource = LogSource.CanDevice,
                            Message = $"写入时出现错误：重试ID{item.Id}"
                        });
                        CanMessages.Add(item);
                    }
                    CanMessages.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    _mediator.Publish (new LogNotification
                    {
                        LogSource = LogSource.CanDevice,
                        Message =$"写入时出现系统错误：{ex.Message}" 
                    });
                }
            }
        }
        public void AddMessage(PCanWriteMessage message)
        {
            CanMessages.Add(message);
        }
        #endregion


        private TPCANStatus CANInit()
        {
           return PCANBasic.Initialize(PcanHandle, m_Baudrate, 0, 0, 0);
            
        }
        public void CLose()
        {
            _tokensource.Cancel();
            PCANBasic.Uninitialize(PcanHandle);
            this.IsReadly = false;

        }


    }
}
