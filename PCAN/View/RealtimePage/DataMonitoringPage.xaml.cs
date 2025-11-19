using PCAN.ViewModel.RunPage;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

namespace PCAN.View.RealtimePage
{
    /// <summary>
    /// DataMonitoringPage.xaml 的交互逻辑
    /// </summary>
    public partial class DataMonitoringPage : Page,IViewFor<DataMonitoringPageViewModel>
    {
        public DataMonitoringPage()
        {
            InitializeComponent();
            this.ViewModel = Locator.Current.GetService<DataMonitoringPageViewModel>();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.WpfPlotGLUserControl, v => v.PlotCon.Content).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PCanClientUsercontrolViewModel, v => v.PCanClientUsercontrol.ViewModel).DisposeWith(d);
              
                #region Command
                //this.BindCommand(ViewModel, vm => vm.LockSendDataCommand, v => v.LockSendDataButton).DisposeWith(d);
                //this.BindCommand(ViewModel, vm => vm.UnLockSendDataCommand, v => v.UnLockSendDataButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartCommand, v => v.StartButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StopCommand, v => v.StopButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RefParmCommand, v => v.RefParmButton).DisposeWith(d);
                #endregion
                #region SendDataComboxSelect
                this.Bind(ViewModel, vm => vm.SendData0, v => v.SendData0Combox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SendData1, v => v.SendData1Combox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SendData2, v => v.SendData2Combox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SendData3, v => v.SendData3Combox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SendData4, v => v.SendData4Combox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SendData5, v => v.SendData5Combox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SendData6, v => v.SendData6Combox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SendData7, v => v.SendData7Combox.SelectedItem).DisposeWith(d);
                #endregion
                #region SendDataComboxDataSource
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.SendData0Combox.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.SendData1Combox.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.SendData2Combox.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.SendData3Combox.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.SendData4Combox.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.SendData5Combox.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.SendData6Combox.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DataMonitoringSettingDataParm, v => v.SendData7Combox.ItemsSource).DisposeWith(d);
                #endregion
                #region Flag
                this.OneWayBind(ViewModel, vm => vm.HasLockSendParm, v => v.SendParmGroup.IsEnabled,b=>!b).DisposeWith(d);
                //this.OneWayBind(ViewModel, vm => vm.HasLockSendParm, v => v.UnLockSendDataButton.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.HasStart, v => v.StartButton.IsEnabled,b=>!b).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.HasStart, v => v.StopButton.IsEnabled,b=>b).DisposeWith(d);
                #endregion
                #region 参数文本
                //this.Bind(ViewModel, vm => vm.GetDataIDText, v => v.GetDataIDTextBox.Text).DisposeWith(d);
                //this.OneWayBind(ViewModel, vm => vm.SendDataText, v => v.SendDataTextBlock.Text).DisposeWith(d);
                //this.Bind(ViewModel, vm => vm.StartIdText, v => v.StartIdTextBox.Text).DisposeWith(d);
                //this.Bind(ViewModel, vm => vm.StartDataText, v => v.StartDataTextBlock.Text).DisposeWith(d);
                //this.Bind(ViewModel, vm => vm.ReciveDataId, v => v.ReciveDataIdTextBlock.Text).DisposeWith(d);
                //this.Bind(ViewModel, vm => vm.StopIdText, v => v.StopIdTextBox.Text).DisposeWith(d);
                //this.Bind(ViewModel, vm => vm.StopDataText, v => v.StopDataTextBlock.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.LimitCount, v => v.LimitCountTextBox.Text).DisposeWith(d);
                #endregion
               
            });
        }
        #region ViewModel
        public DataMonitoringPageViewModel ViewModel
        {
            get { return (DataMonitoringPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (DataMonitoringPageViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DataMonitoringPageViewModel), typeof(DataMonitoringPage), new PropertyMetadata(null));
        #endregion

        private void ConnectionCanButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.CanConnectiontoolbar.Visibility==Visibility.Hidden)
            {
                this.CanConnectiontoolbar.Visibility = Visibility.Visible;
            }
            else
            {
                this.CanConnectiontoolbar.Visibility = Visibility.Hidden;
            }
           
        }
    }
}
