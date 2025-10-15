using PCAN.View.UserPage;
using PCAN.ViewModel;
using PCAN.ViewModel.USercontrols;
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
    /// PCanClientUsercontrol.xaml 的交互逻辑
    /// </summary>
    public partial class PCanClientUsercontrol : UserControl,IViewFor<PCanClientUsercontrolViewModel>
    {
        public PCanClientUsercontrol()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Ports, v => v.PortsList.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedPort, v => v.PortsList.SelectedValue).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.LocalBaudRates, v => v.BaudRatesList.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedBaudrate, v => v.BaudRatesList.SelectedValue).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.DeviceID, v => v.DeviceIDList.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ConnectCommand, v => v.Connect).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RefreshPortCommand, v => v.RefreshPort).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UnConnectCommand, v => v.UnConnect).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NoConnected, v => v.Connect.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsConnected, v => v.UnConnect.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NoConnected, v => v.RefreshPort.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ConnectLab, v => v.ConnectLab.Content).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FrameInterval, v => v.FrameInterval.Text).DisposeWith(d);
            });
        }
        #region ViewModel
        public PCanClientUsercontrolViewModel ViewModel
        {
            get { return (PCanClientUsercontrolViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (PCanClientUsercontrolViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(PCanClientUsercontrolViewModel), typeof(PCanClientUsercontrol), new PropertyMetadata(null));
        #endregion
    }
}
