using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCAN.Drive;
using PCAN.Drive.Modle;

using PCAN.Notification.Log;
using PCAN.Shard.Models;
using PCAN.Shard.Modles;
using PCAN_AutoCar_Test_Client.Models;
using Peak.Can.Basic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace PCAN_AutoCar_Test_Client.ViewModel.USercontrols
{
    public class PCanClientUsercontrolViewModel:ReactiveObject
    {
        private CANSetting _canSettings;
        private readonly CanFileSet _canfileset;

        public PCanClientUsercontrolViewModel(ILogger<PCanClientUsercontrolViewModel> logger,
            IMediator mediator,IOptions<CANSetting> cansettingsoptions,IOptions<CanFileSet> canfilesetoptions)
        {
            _logger = logger;
            _mediator = mediator;
            _canfileset = canfilesetoptions.Value;
            _canSettings = cansettingsoptions.Value;

            this.RefreshPortCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    Ports.Clear();
                    UInt16 deviceID = 1;
                    uint iChannelsCount;
                    var stsResult = Api.GetValue(PcanChannel.None, PcanParameter.AttachedChannelsCount, out iChannelsCount);
                    if (stsResult == PcanStatus.OK)
                    {
                        PcanChannelInformation[] info = new PcanChannelInformation[iChannelsCount];

                        stsResult = Api.GetValue(PcanChannel.None, PcanParameter.AttachedChannelsInformation, info);
                        if (stsResult == PcanStatus.OK)
                        {
                            foreach (var channel in info)
                                if ((channel.ChannelCondition & ChannelCondition.ChannelAvailable) == ChannelCondition.ChannelAvailable)
                                {
                                    Ports.Add(new LocalPorts() { PortName = channel.DeviceName, PortsNum = (ushort)channel.ChannelHandle });
                                }
                            mediator.Publish(new LogNotification()
                            {
                                LogSource = LogSource.CanDevice,
                                Message = $"刷新端口成功,共{Ports.Count}个端口"
                            });
                            if (Ports.Count>0)
                            {
                                this.SelectedPort = Ports[0].PortsNum;

                            }
                        }


                    }
                }
                catch (Exception ex)
                {

                    mediator.Publish(new LogNotification()
                    {
                        LogLevel = LogLevel.Error,
                        LogSource = LogSource.CanDevice,
                        Message = $"刷新端口失败,{ex.Message}"
                    });
                }
                
            });

            this.ConnectCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(DeviceID) && !UseCANFD)
                    {
                        MessageBox.Show("设备ID不能为空");
                        return;
                    }
                    if (CanDrive != null)
                    {
                        MessageBox.Show("已连接设备，请先断开");
                        return;
                    }
                    //logger.LogDebug($"{SelectedPort}:{SelectedBaudrate}");
                    if (UseCANFD)
                    {
                        CanDrive = new CANDrive(SelectedPort, Convert.ToUInt32(DeviceID, 16), SelectedBaudrateFD, _mediator, FrameInterval, useFD: true);
                        
                    }
                    else
                    {
                        CanDrive = new CANDrive(SelectedPort, Convert.ToUInt32(DeviceID, 16), SelectedBaudrate, _mediator, FrameInterval);

                    }
                    CanDrive.FilterMessages(Convert.ToUInt32( _canfileset.FromId,16),Convert.ToUInt32( _canfileset.ToId,16));
                    this.CanDrive.CANReadMsg.ObserveOn(RxApp.MainThreadScheduler).Subscribe(msg =>
                    {
                        NewMessage.Value = msg;
                        var oldmsg = TPCANMsgs.FirstOrDefault(x => x.ID == msg.ID);
                        if (oldmsg != null)
                        {
                            oldmsg.MSGTYPE = msg.MSGTYPE;
                            oldmsg.LEN = msg.LEN;
                            oldmsg.DATA = msg.DATA;
                            oldmsg.Count++;
                        }
                        else
                        {
                            TPCANMsgs.Add(msg);

                        }

                    });
                    _logger.LogInformation("连接设备");
                    IsConnected = true;
                    ConnectLab = "已连接";
                    NoConnected = false;
                }
                catch (Exception ex)
                {

                    mediator.Publish(new LogNotification()
                    {
                        LogLevel = LogLevel.Error,
                        LogSource = LogSource.CanDevice,
                        Message = $"连接出现错误:{ex.Message}"
                    });
                }
                
            });
            this.UnConnectCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    if (CanDrive == null)
                    {
                        MessageBox.Show("未连接设备");
                        return;
                    }
                    CanDrive.CLose();
                    CanDrive = null;
                    _logger.LogDebug("断开设备");
                    IsConnected = false;
                    ConnectLab = "未连接";
                    NoConnected = true;
                }
                catch (Exception ex)
                {

                    mediator.Publish(new LogNotification()
                    {
                        LogLevel = LogLevel.Error,
                        LogSource = LogSource.CanDevice,
                        Message = $"断开连接出现错误:{ex.Message}"
                    });
                }
                
            });
           
            this.RefreshPortCommand.Execute(null);
            foreach (var fdconstr in _canSettings.FDConStrEx)
            {
                LocalFDBaudRates.Add(new LocalFDBaudRate()
                {
                    Baudrate = fdconstr.ConStr,
                    BaudRateName = fdconstr.Name
                });
            }
            if (_canSettings.FDConStrEx.Count>0)
            {
                SelectedBaudrateFD = _canSettings.FDConStrEx[0].ConStr;
            }
            this.UseCANFD = _canSettings.UseFD;

        }
        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="useextended"></param>
        /// <param name="action"></param>
        public bool WriteMsg(uint id, byte[] data,bool useextended,Action? action=null)
        {
            
            if (CanDrive == null)
            {
                MessageBox.Show("请先连接设备");
                return false;
            }
            CanDrive.AddMessage(new PCanWriteMessage() { Data = data,MessageType= useextended?MessageType.Extended:MessageType.Standard, Id = id });
            if (action != null)
            {
                action();

            }
            return true;
           
        }
        private CANDrive CanDrive;
        private readonly ILogger<PCanClientUsercontrolViewModel> _logger;
        private readonly IMediator _mediator;

        [Reactive]
        public bool IsConnected { get; set; }
        public ushort SelectedPort { get; set; }
        /// <summary>
        /// 每帧间隔时间，单位ms
        /// </summary>
        public int FrameInterval { get; set; } = 10;
        [Reactive]
        public string ConnectLab { get; set; } = "未连接";
        [Reactive]
        public bool NoConnected { get; set; } = true;

        [Reactive]
        public bool UseCANFD { get; set; } = false;
       
        public Bitrate SelectedBaudrate { get; set; }
        public string SelectedBaudrateFD { get; set; }= BitrateFD.BitrateSaeJ2284_4;
        public string DeviceID { get; set; }
        public ReactiveProperty<ReadMessage> NewMessage { get; set; } = new ReactiveProperty<ReadMessage>();
        public ObservableCollection<LocalPorts> Ports { get; set; } = [];
        public ObservableCollection<ReadMessage> TPCANMsgs { get; set; } = [];
        public ObservableCollection<LocalBaudRate> LocalBaudRates { get; set; } =
     [
        new LocalBaudRate()
            {
                Baudrate= Bitrate.Pcan1000,
                BaudRateName="1M  kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= Bitrate.Pcan800,
                BaudRateName="800 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= Bitrate.Pcan500,
                BaudRateName="500 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= Bitrate.Pcan250,
                BaudRateName="250 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= Bitrate.Pcan125,
                BaudRateName="125 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= Bitrate.Pcan100,
                BaudRateName="100 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= Bitrate.Pcan50,
                BaudRateName="50  kBit/sec"
            },

        ];
        public ObservableCollection<LocalFDBaudRate> LocalFDBaudRates { get; set; } =
        [
            new LocalFDBaudRate()
            {
                Baudrate= BitrateFD.BitrateSaeJ2284_5,
                BaudRateName="BitrateSaeJ2284_5"
            },
            new LocalFDBaudRate()
            {
                Baudrate= BitrateFD.BitrateSaeJ2284_4,
                BaudRateName="BitrateSaeJ2284_4"
            },
           //new LocalFDBaudRate()
           // {
           //     Baudrate= BitrateFD.greenworks_40Mhz_500k_2M,
           //     BaudRateName="greenworks_40Mhz_500k_2M"
           // },

        ];
        public ICommand ConnectCommand { get; }
        public ICommand UnConnectCommand { get; }
        public ICommand RefreshPortCommand { get; }
     

    }
}
