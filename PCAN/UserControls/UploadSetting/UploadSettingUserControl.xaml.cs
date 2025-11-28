using PCAN.UserControls.DataMonitoringSettings;
using PCAN.ViewModel.Usercontrols.DataMonitoringSettings;
using PCAN.ViewModel.Usercontrols.UploadSetting;
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

namespace PCAN.UserControls.UploadSetting
{
    /// <summary>
    /// UploadSettingUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class UploadSettingUserControl : UserControl,IViewFor<UploadSettingUserControlViewModel>
    {
        public UploadSettingUserControl()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.MaxResendCount, v => v.MaxResendCountTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.TimeOutSeconds, v => v.TimeoutTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.PackageSize, v => v.PackSizeTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsHexUpload, v => v.HexUploadCheckBox.IsChecked).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RefCommand, v => v.RefUploadSettingsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveCommand, v => v.SaveUploadSettingsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.EncryptionFileCommand, v => v.EncryptionButton).DisposeWith(d);

            });
        }
        #region ViewModel
        public UploadSettingUserControlViewModel ViewModel
        {
            get { return (UploadSettingUserControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (UploadSettingUserControlViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(UploadSettingUserControlViewModel), typeof(UploadSettingUserControl), new PropertyMetadata(null));
        #endregion
    }
}
