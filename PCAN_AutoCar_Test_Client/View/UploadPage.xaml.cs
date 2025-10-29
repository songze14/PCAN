using PCAN_AutoCar_Test_Client.ViewModel;
using ReactiveUI;
using Splat;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;

namespace PCAN_AutoCar_Test_Client.View
{
    /// <summary>
    /// UploadPage.xaml 的交互逻辑
    /// </summary>
    public partial class UploadPage : Page,IViewFor<UploadPageViewModel>
    {
        public UploadPage()
        {
            InitializeComponent();
            ViewModel=Locator.Current.GetService<UploadPageViewModel>();
            this.DataContext = ViewModel;
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.PCanClientUsercontrolViewModel, v => v.PcanClientUsercontrol.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SelectedFilePath, v => v.FilePathTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.UploadDevices, v => v.UploadDeviceComboBox.ItemsSource).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.BrowseFileCommand, v => v.BrowseFileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UploadCommand, v => v.UploadFileButton).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedUploadDeviceValue, v => v.UploadDeviceComboBox.SelectedValue).DisposeWith(d);
                //this.Bind(ViewModel,vm=>vm.MaxResendCount,v=>v.RetryCountTextBox.Text).DisposeWith(d);
                //this.Bind(ViewModel, vm => vm.TimeOutSeconds, v => v.TimeoutTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.UploadDataGridModels, v => v.UploadDataGrid.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel,vm=>vm.UploadProgress,v=>v.UploadProgressBar.Value).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.UploadProgress, v => v.UploadProgressLable.Content).DisposeWith(d);
                //this.BindCommand(ViewModel, vm => vm.EncryptionFileCommand, v => v.EncryptionButton).DisposeWith(d);
            });
        }
        #region ViewModel
        public UploadPageViewModel ViewModel
        {
            get { return (UploadPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (UploadPageViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(UploadPageViewModel), typeof(UploadPage), new PropertyMetadata(null));
        #endregion
    }
}
