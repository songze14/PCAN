using DynamicData;
using DynamicData.Binding;
using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.Modles;
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
            DataMonitoringSettingDataParmSourceList
              .Connect()
              .Sort(SortExpressionComparer<DataMonitoringSettingDataParm>.Ascending(x => x.Index)) // 排序
             
              .Bind(out _dataMonitoringSettingDataParmSourceList)
              .Subscribe();
            this.LockSendDataCommand = ReactiveCommand.Create(() =>
            {
                var receivedatalenght = 0;
                var senddatatext = new StringBuilder();
                senddatatext = SendData0 != null ? UpdateSendData(ref receivedatalenght, SendData0.Size, SendData0.Index,  senddatatext):UpdateSendData( ref receivedatalenght,0,0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData1 != null ? UpdateSendData(ref receivedatalenght, SendData1.Size, SendData1.Index,  senddatatext):UpdateSendData( ref receivedatalenght,0,0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData2 != null ? UpdateSendData(ref receivedatalenght, SendData2.Size, SendData2.Index, senddatatext) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData3 != null ? UpdateSendData(ref receivedatalenght, SendData3.Size, SendData3.Index, senddatatext) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData4 != null ? UpdateSendData(ref receivedatalenght, SendData4.Size, SendData4.Index, senddatatext) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData5 != null ? UpdateSendData(ref receivedatalenght, SendData5.Size, SendData5.Index, senddatatext) : UpdateSendData(ref receivedatalenght, 0, 0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData6 != null ? UpdateSendData(ref receivedatalenght, SendData6.Size, SendData6.Index,  senddatatext):UpdateSendData( ref receivedatalenght,0,0, senddatatext);
                senddatatext.Append('-');
                senddatatext = SendData7 != null ? UpdateSendData(ref receivedatalenght, SendData7.Size, SendData7.Index,  senddatatext):UpdateSendData( ref receivedatalenght,0,0, senddatatext);
              
                SendDataText = senddatatext.ToString();
            });
                GetDataMonitoringSettingDataParmSourceList();
        }
        private async Task GetDataMonitoringSettingDataParmSourceList()
        {
            DataMonitoringSettingDataParmSourceList.Add(new SqlLite.Model.DataMonitoringSettingDataParm()
            {
                Index = 0,
                Name = "无",
                Type = "None"
            });
            var result =await _datamonitoringsettingservice.GetDataMonitoringSettingDataParms();
            DataMonitoringSettingDataParmSourceList.Clear();
            DataMonitoringSettingDataParmSourceList.AddRange(result);
        }
        private StringBuilder UpdateSendData(ref int receivedatalenght,int addreceivedatalenght,int addsenddatatextIndex,  StringBuilder senddatatext)
        {
            receivedatalenght+=addreceivedatalenght;
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
        #endregion
        #region 参数文本
        [Reactive]
        public string SendDataText { get; set; }
        #endregion
        public WpfPlotGLUserControl WpfPlotGLUserControl { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; set; }
      
    }
}
