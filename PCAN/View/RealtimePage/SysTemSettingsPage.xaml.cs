using PCAN.ViewModel.RunPage;
using ReactiveUI;
using Splat;
using System.Windows;
using System.Windows.Controls;

namespace PCAN.View.RealtimePage
{
    /// <summary>
    /// SysTemSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SysTemSettingsPage : Page,IViewFor<SysTemSettingsPageViewModel>
    {
        public SysTemSettingsPage()
        {
            ViewModel=Locator.Current.GetService<SysTemSettingsPageViewModel>();
            InitializeComponent();
        }
        #region ViewModel
        public SysTemSettingsPageViewModel ViewModel
        {
            get { return (SysTemSettingsPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (SysTemSettingsPageViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(SysTemSettingsPageViewModel), typeof(SysTemSettingsPage), new PropertyMetadata(null));
        #endregion
    }
}
