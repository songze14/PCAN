using PCAN.Modles;
using PCAN.ViewModel;
using PCAN.ViewModel.Window;
using ReactiveUI;
using Splat;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace PCAN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,IViewFor<MainWindowViewModel>
    {
        public MainWindow(AppViewModel appViewModle)
        {
           
            InitializeComponent();
            this.ViewModel=Locator.Current.GetService<MainWindowViewModel>();
            AppViewModle = appViewModle;
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.UILogsViewModel, v => v.uilogView.ViewModel).DisposeWith(d);
            });
            this.AppViewModle.CurrentPage.ObserveOn(RxApp.MainThreadScheduler).Subscribe(page =>
            {
                if (page != null)
                {
                    this.navWin.Navigate(page);
                }
            });
            AppViewModle.NavigateTo(UrlDefines.URL_BasicFunctions);
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
        public  AppViewModel AppViewModle{get;set;}


        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AppViewModle.NavigateTo(UrlDefines.URL_BasicFunctions);

        }

        private void PCANDataParse_Click(object sender, RoutedEventArgs e)
        {
            AppViewModle.NavigateTo(UrlDefines.URL_PCANDataParse);

        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            AppViewModle.NavigateTo(UrlDefines.URL_Upload);

        }

        private void DeviceParmTuning_Click(object sender, RoutedEventArgs e)
        {
            AppViewModle.NavigateTo(UrlDefines.URL_DeviceParmTuning);

        }

        private void DataMonitoring_Click(object sender, RoutedEventArgs e)
        {
            AppViewModle.NavigateTo(UrlDefines.URL_DataMonitoring);
        }

        private void SysTemSettings_Click(object sender, RoutedEventArgs e)
        {
            AppViewModle.NavigateTo(UrlDefines.URL_SysTemSettings);
        }
    }
  
}