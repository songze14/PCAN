using PCAN.ViewModel.RunPage;
using ReactiveUI;
using Splat;
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
    }
}
