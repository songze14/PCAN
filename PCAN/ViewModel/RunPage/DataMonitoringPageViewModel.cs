using DynamicData;
using DynamicData.Binding;
using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.Modles;
using PCAN.Notification.Log;
using PCAN.Shard.Tools;
using PCAN.SqlLite.Abs;
using PCAN.SqlLite.Model;
using PCAN.UserControls;
using PCAN.ViewModel.USercontrols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unit = System.Reactive.Unit;

namespace PCAN.ViewModel.RunPage
{
    public class DataMonitoringPageViewModel: ReactiveObject
    {
        private readonly ILogger<BasicFunctionsPageViewModel> _logger;
        private readonly IMediator _mediator;
        private readonly IDataMonitoringSettingService _datamonitoringsettingservice;

        public DataMonitoringPageViewModel(WpfPlotGLUserControl wpfPlotGLUserControl, 
            ILogger<BasicFunctionsPageViewModel> logger, 
            IMediator mediator, 
            PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel,
            IDataMonitoringSettingService dataMonitoringSettingService)
        {
            WpfPlotGLUserControl = wpfPlotGLUserControl;
            _logger = logger;
            _mediator = mediator;
            PCanClientUsercontrolViewModel = pCanClientUsercontrolViewModel;
            _datamonitoringsettingservice = dataMonitoringSettingService;
            PCanClientUsercontrolViewModel.NewMessage.Subscribe(msg =>
            {
                try
                {
                    if (msg == null)
                        return;
                    var id =  msg.ID.ToString("X");
                    if (id == _reciveDataId && HasStart)
                    {
                        var index = 0;
                        foreach (var item in DataMonitoringSettingDataParmList)
                        {
                            if (item.Index != 0)
                            {
                                var datastr = CTypeToCsharpTypeValue.GetParmValue( item.Type,msg.DATA[index..(index + item.Size)]);
                                if (datastr != null)
                                {
                                    var datavalue = Convert.ToDouble(datastr);
                                    PlotDics[item.Name].Add(datavalue);
                                }
                            }
                            index += item.Size;
                        }
                        WpfPlotGLUserControl.SetLimit(LimitCount);
                    }
                }
                catch (Exception ex)
                {
                  _mediator.Publish (new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.DataMonitoring, Message = $"数据解析错误:{ex.Message}" });
                }
            });
            DataMonitoringSettingDataParmSourceList
              .Connect()
              .Sort(SortExpressionComparer<DataMonitoringSettingDataParm>.Ascending(x => x.Index)) // 排序
             
              .Bind(out _dataMonitoringSettingDataParmSourceList)
              .Subscribe();
            this.LockSendDataCommand = ReactiveCommand.Create(() =>
            {
                DataMonitoringSettingDataParmList.Clear();
                var receivedatalenght = 0;
                var senddatatext = new StringBuilder();
                senddatatext = SendData0 != null ? UpdateSendData(ref receivedatalenght, SendData0.Size, SendData0.Index, senddatatext, SendData0) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData1 != null ? UpdateSendData(ref receivedatalenght, SendData1.Size, SendData1.Index, senddatatext, SendData1) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData2 != null ? UpdateSendData(ref receivedatalenght, SendData2.Size, SendData2.Index, senddatatext, SendData2) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData3 != null ? UpdateSendData(ref receivedatalenght, SendData3.Size, SendData3.Index, senddatatext, SendData3) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData4 != null ? UpdateSendData(ref receivedatalenght, SendData4.Size, SendData4.Index, senddatatext, SendData4) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData5 != null ? UpdateSendData(ref receivedatalenght, SendData5.Size, SendData5.Index, senddatatext, SendData5) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData6 != null ? UpdateSendData(ref receivedatalenght, SendData6.Size, SendData6.Index, senddatatext, SendData6) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData7 != null ? UpdateSendData(ref receivedatalenght, SendData7.Size, SendData7.Index, senddatatext, SendData7) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);

                SendDataText = senddatatext.ToString();
                if (receivedatalenght>8)
                {
                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.DataMonitoring, Message = "预解析数据长度超过8" });
                    return;
                }
                var startbye = new byte[8];
                int driveid = Convert.ToUInt16(PCanClientUsercontrolViewModel.DeviceID, 16);
                var dirveridBytes = BitConverter.GetBytes((ushort)driveid);
                dirveridBytes.CopyTo(startbye, 0);
                startbye[dirveridBytes.Length] = (byte)(dirveridBytes.Length+4);
                startbye[dirveridBytes.Length + 1] = 0;
                startbye[dirveridBytes.Length + 2] = 0xA5;
                startbye[dirveridBytes.Length + 3] = 0xA5;
                StartDataText= BitConverter.ToString(startbye);

                var stopbye = new byte[8];
                dirveridBytes.CopyTo(stopbye, 0);
                stopbye[dirveridBytes.Length] = (byte)(dirveridBytes.Length + 4);
                stopbye[dirveridBytes.Length + 1] = 0;
                stopbye[dirveridBytes.Length + 2] = 0xCA;
                stopbye[dirveridBytes.Length + 3] = 0xCA;
                StopDataText = BitConverter.ToString(stopbye);
                HasLockSendParm = true;
            });
            this.UnLockSendDataCommand= ReactiveCommand.Create(() =>
            {
                HasLockSendParm = false;
            });
            this.StartCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    if (!HasLockSendParm)
                    {
                        _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.DataMonitoring, Message = "请先锁定发送参数" });
                        return;
                    }
                    WpfPlotGLUserControl.ClearAllSignal();
                    PlotDics.Clear();
                    foreach (var item in DataMonitoringSettingDataParmList)
                    {
                        if (item.Index != 0)
                        {
                            var datalist = new List<double>();
                            WpfPlotGLUserControl.AddSignal(datalist, item.Name);
                            PlotDics.Add(item.Name, datalist);
                        }
                    }
                    var startid = uint.Parse(StartIdText, System.Globalization.NumberStyles.HexNumber);
                    var datastr = StartDataText.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    byte[] startdata;
                    if (datastr != null)
                    {
                        startdata = new byte[datastr.Length];
                        for (int i = 0; i < datastr.Length; i++)
                        {
                            startdata[i] = Convert.ToByte(datastr[i], 16);
                        }
                    }
                    else
                    {
                        //无数据
                        startdata = [];
                    }
                    PCanClientUsercontrolViewModel.WriteMsg(startid, startdata);
                    byte[] getdatasenddata;
                    var getdatasenddatastr = SendDataText.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    if (getdatasenddatastr != null)
                    {
                        getdatasenddata = new byte[getdatasenddatastr.Length];
                        for (int i = 0; i < getdatasenddatastr.Length; i++)
                        {
                            getdatasenddata[i] = Convert.ToByte(getdatasenddatastr[i], 16);
                        }
                    }
                    else
                    {
                        //无数据
                        getdatasenddata = [];
                    }
                    var getdataid = uint.Parse(GetDataIDText, System.Globalization.NumberStyles.HexNumber);
                    PCanClientUsercontrolViewModel.WriteMsg(getdataid, getdatasenddata);
                    PCanClientUsercontrolViewModel.Reset();
                    HasStart = true;
                }
                catch (Exception ex)
                {

                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.DataMonitoring, Message = $"开始时出现错误:{ex.Message}" });

                }



            });
            this.StopCommand = ReactiveCommand.Create(() =>
            {
                var stopid = uint.Parse(StartIdText, System.Globalization.NumberStyles.HexNumber);
                var datastr = StopDataText.Split('-', StringSplitOptions.RemoveEmptyEntries);
                byte[] startdata;
                if (datastr != null)
                {
                    startdata = new byte[datastr.Length];
                    for (int i = 0; i < datastr.Length; i++)
                    {
                        startdata[i] = Convert.ToByte(datastr[i], 16);
                    }
                }
                else
                {
                    //无数据
                    startdata = [];
                }
                PCanClientUsercontrolViewModel.WriteMsg(stopid, startdata);
                HasStart = false;
            });
            GetDataMonitoringSettingDataParmSourceList();
        }
        private async Task GetDataMonitoringSettingDataParmSourceList()
        {
            DataMonitoringSettingDataParmSourceList.Clear();

            DataMonitoringSettingDataParmSourceList.Add(new SqlLite.Model.DataMonitoringSettingDataParm()
            {
                Index = 0,
                Name = "无",
                Type = "None",
                Size=0,
            });
            var result =await _datamonitoringsettingservice.GetDataMonitoringSettingDataParms();
            DataMonitoringSettingDataParmSourceList.AddRange(result);
        }
        private StringBuilder UpdateSendData(ref int receivedatalenght,int addreceivedatalenght,int addsenddatatextIndex, StringBuilder senddatatext, DataMonitoringSettingDataParm? dataParm=null)
        {
            receivedatalenght+=addreceivedatalenght;
            if (dataParm!=null)
            {
                DataMonitoringSettingDataParmList.Add(dataParm);
            }
            return senddatatext.Append(addsenddatatextIndex.ToString("00"));
        }
        #region SendDataComboxSelect
        [Reactive]
        public DataMonitoringSettingDataParm SendData0 { get; set; } = new DataMonitoringSettingDataParm();
        [Reactive]
        public DataMonitoringSettingDataParm SendData1 { get; set; } = new DataMonitoringSettingDataParm();
        [Reactive]
        public DataMonitoringSettingDataParm SendData2 { get; set; } = new DataMonitoringSettingDataParm();
        [Reactive]
        public DataMonitoringSettingDataParm SendData3 { get; set; } = new DataMonitoringSettingDataParm();
        [Reactive]
        public DataMonitoringSettingDataParm SendData4 { get; set; } = new DataMonitoringSettingDataParm();
        [Reactive]
        public DataMonitoringSettingDataParm SendData5 { get; set; } = new DataMonitoringSettingDataParm();
        [Reactive]
        public DataMonitoringSettingDataParm SendData6 { get; set; } = new DataMonitoringSettingDataParm();
        [Reactive]
        public DataMonitoringSettingDataParm SendData7 { get; set; } = new DataMonitoringSettingDataParm();

        #endregion

        #region SendDataComboxDataSource
        public SourceList<DataMonitoringSettingDataParm> DataMonitoringSettingDataParmSourceList { get; } = new();
        private readonly ReadOnlyObservableCollection<DataMonitoringSettingDataParm> _dataMonitoringSettingDataParmSourceList;
        public ReadOnlyObservableCollection<DataMonitoringSettingDataParm> DataMonitoringSettingDataParm => _dataMonitoringSettingDataParmSourceList;
        #endregion
        #region Command
        public ReactiveCommand<Unit,Unit> LockSendDataCommand { get; }
        public ReactiveCommand<Unit,Unit> UnLockSendDataCommand { get; }
        public ReactiveCommand<Unit,Unit> StartCommand { get; }
        public ReactiveCommand<Unit,Unit> StopCommand { get; }
        #endregion
        #region Flag
        [Reactive]
        public bool HasLockSendParm { get; set; }
        #endregion
        #region 参数文本

        [Reactive]
        public string GetDataIDText { get; set; } = "3f3";
        [Reactive]
        public string SendDataText { get; set; }
        [Reactive]
        public string StartIdText { get; set; } = "3f3";
        [Reactive]
        public string StartDataText { get; set; }

        [Reactive]
        public string StopIdText { get; set; } = "3f3";
        [Reactive]
        public string StopDataText { get; set; }
        private string _reciveDataId =>ReciveDataId.ToUpper();

        [Reactive]
        public string ReciveDataId { get; set; } = "3ff";
        [Reactive]
        public int LimitCount { get; set; } = 1000;
        #endregion
        #region 本地变量
        private Dictionary<string, List<double>> PlotDics = new();
        private List<DataMonitoringSettingDataParm> DataMonitoringSettingDataParmList = new();
        [Reactive]
        public bool HasStart { get; set; }

        #endregion
        public WpfPlotGLUserControl WpfPlotGLUserControl { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; set; }
      
    }
}
