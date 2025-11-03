using PCAN_AutoCar_Test_Client.Models;
using PCAN_AutoCar_Test_Client.ViewModel;
using ReactiveUI;
using Splat;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PCAN_AutoCar_Test_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
    {
        public MainWindow(AppViewModel appViewModle )
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<MainWindowViewModel>();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.UILogsViewModel, v => v.uilogView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Title, v => v.TitleTextblock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Version, v => v.Title).DisposeWith(d);
            });
            AppViewModle = appViewModle;
            AppViewModle.NavigateTo(UrlDefines.URL_Test);
            this.AppViewModle.CurrentPage.ObserveOn(RxApp.MainThreadScheduler).Subscribe(page =>
            {
                if (page != null)
                {
                    this.navWin.Navigate(page);
                }
            });
        }
        #region ViewModel
        public MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (MainWindowViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainWindowViewModel), typeof(MainWindow), new PropertyMetadata(null));
        #endregion
        public AppViewModel AppViewModle { get; set; }

    }
}