using PCAN.View.RealtimePage;
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

namespace PCAN.View.UserPage
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
                this.OneWayBind(ViewModel, vm => vm.MCU, v => v.MCUTextBox.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.BrowseFileCommand, v => v.BrowseFileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UploadCommand, v => v.UploadFileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ReloadCommand, v => v.ReloadButton).DisposeWith(d);
                this.Bind(ViewModel,vm=>vm.MaxResendCount,v=>v.RetryCountTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.TimeOutSeconds, v => v.TimeoutTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.UploadDataGridModels, v => v.UploadDataGrid.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel,vm=>vm.UploadProgress,v=>v.UploadProgressBar.Value).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.UploadProgress, v => v.UploadProgressLable.Content).DisposeWith(d);
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
