using PCAN.ViewModel.Usercontrols.DataMonitoringSettings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PCAN.UserControls.DataMonitoringSettings
{
    /// <summary>
    /// DataMonitoringSettingUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DataMonitoringPlotParmUserControl : UserControl,IViewFor<DataMonitoringPlotParmUserControlViewModel>
    {
        public DataMonitoringPlotParmUserControl()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.RefreshParmCommand, v => v.RefreshPlotParmButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AnalysisParmstrCommand, v => v.AnalysisParmstrButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SavePlotParmCommand, v => v.SavePlotParmButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveParmCommand, v => v.SaveParmButton).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.DataParmDataGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.DeviceParmValueStr, v => v.DeviceParmValueStrTextBox.Text).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.GetDataIDText, v => v.GetDataIDTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.StartIdText, v => v.StartIdTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ReciveDataId, v => v.ReciveDataIdTextBlock.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.StopIdText, v => v.StopIdTextBox.Text).DisposeWith(d);
            });
            
        }
        #region ViewModel
        public DataMonitoringPlotParmUserControlViewModel ViewModel
        {
            get { return (DataMonitoringPlotParmUserControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (DataMonitoringPlotParmUserControlViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DataMonitoringPlotParmUserControlViewModel), typeof(DataMonitoringPlotParmUserControl), new PropertyMetadata(null));
        #endregion
    }
}
