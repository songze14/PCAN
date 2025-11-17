using PCAN.ViewModel.Usercontrols;
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

namespace PCAN.UserControls
{
    /// <summary>
    /// DataMonitoringSettingUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DataMonitoringSettingUserControl : UserControl,IViewFor<DataMonitoringSettingUserControlViewModel>
    {
        public DataMonitoringSettingUserControl()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.RefreshParmCommand, v => v.RefreshParmButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AnalysisParmstrCommand, v => v.AnalysisParmstrButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveParmCommand, v => v.SaveParmButton).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.DataParmDataGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.DeviceParmValueStr, v => v.DeviceParmValueStrTextBox.Text).DisposeWith(d);
            });
            
        }
        #region ViewModel
        public DataMonitoringSettingUserControlViewModel ViewModel
        {
            get { return (DataMonitoringSettingUserControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (DataMonitoringSettingUserControlViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DataMonitoringSettingUserControlViewModel), typeof(DataMonitoringSettingUserControl), new PropertyMetadata(null));
        #endregion
    }
}
