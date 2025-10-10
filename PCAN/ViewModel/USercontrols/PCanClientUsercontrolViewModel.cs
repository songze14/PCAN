using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.Drive;
using PCAN.Drive.Modle;
using PCAN.Modles;
using Peak.Can.Basic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PCAN.ViewModel.USercontrols
{
    public class PCanClientUsercontrolViewModel:ReactiveObject
    {
        public PCanClientUsercontrolViewModel(ILogger<PCanClientUsercontrolViewModel> logger,IMediator mediator)
        {
            this.RefreshPortCommand = ReactiveCommand.Create(() =>
            {
                Ports.Clear();
                UInt16 deviceID = 1;
                uint iChannelsCount;
                var stsResult = PCANBasic.GetValue(PCANBasic.PCAN_NONEBUS, TPCANParameter.PCAN_ATTACHED_CHANNELS_COUNT, out iChannelsCount, sizeof(uint));
                if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                {
                    TPCANChannelInformation[] info = new TPCANChannelInformation[iChannelsCount];

                    stsResult = PCANBasic.GetValue(PCANBasic.PCAN_NONEBUS, TPCANParameter.PCAN_ATTACHED_CHANNELS, info);
                    if (stsResult == TPCANStatus.PCAN_ERROR_OK)

                        foreach (var channel in info)
                            if ((channel.channel_condition & PCANBasic.PCAN_CHANNEL_AVAILABLE) == PCANBasic.PCAN_CHANNEL_AVAILABLE)
                            {
                                var bIsFD = (channel.device_features & PCANBasic.FEATURE_FD_CAPABLE) == PCANBasic.FEATURE_FD_CAPABLE;
                                Ports.Add(new LocalPorts() { PortName = channel.device_name, PortsNum = channel.channel_handle });
                            }
                }
            });

            this.ConnectCommand = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrWhiteSpace(DeviceID))
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
                CanDrive = new CANDrive(SelectedPort, Convert.ToUInt32(DeviceID, 16), SelectedBaudrate, _mediator, FrameInterval);
                this.CanDrive.CANMsg.ObserveOn(RxApp.MainThreadScheduler).Subscribe(msg =>
                {
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
            });
            this.UnConnectCommand = ReactiveCommand.Create(() =>
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
            });

            this.RefreshPortCommand.Execute(null);
            _logger = logger;
            _mediator = mediator;
        }
        private CANDrive CanDrive;
        private bool _isConnected;
        private readonly ILogger<PCanClientUsercontrolViewModel> _logger;
        private readonly IMediator _mediator;

        [Reactive]
        public bool IsConnected { get; set; }
        public ushort SelectedPort { get; set; }
        public int FrameInterval { get; set; } = 10;
        [Reactive]
        public string ConnectLab { get; set; } = "未连接";
        [Reactive]
        public bool NoConnected { get; set; } = true;

        public TPCANBaudrate SelectedBaudrate { get; set; }
        public string DeviceID { get; set; }
        public ObservableCollection<LocalPorts> Ports { get; set; } = [];
        public ObservableCollection<ReadMessage> TPCANMsgs { get; set; } = [];
        public ObservableCollection<LocalBaudRate> LocalBaudRates { get; set; } =
     [
         new LocalBaudRate()
            {
                Baudrate= TPCANBaudrate.PCAN_BAUD_1M,
                BaudRateName="1M  kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= TPCANBaudrate.PCAN_BAUD_800K,
                BaudRateName="800 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= TPCANBaudrate.PCAN_BAUD_500K,
                BaudRateName="500 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= TPCANBaudrate.PCAN_BAUD_250K,
                BaudRateName="250 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= TPCANBaudrate.PCAN_BAUD_125K,
                BaudRateName="125 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= TPCANBaudrate.PCAN_BAUD_100K,
                BaudRateName="100 kBit/sec"
            },
            new LocalBaudRate()
            {
                Baudrate= TPCANBaudrate.PCAN_BAUD_50K,
                BaudRateName="50  kBit/sec"
            },

        ];
        public ICommand ConnectCommand { get; }
        public ICommand UnConnectCommand { get; }
        public ICommand RefreshPortCommand { get; }

    }
}
