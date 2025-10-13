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
    /// ParmValueSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ParmValueSettingPage : Page,IViewFor<ParmValueSettingPageViewModel>
    {
        public ParmValueSettingPage()
        {
            InitializeComponent();
            ViewModel= Locator.Current.GetService<ParmValueSettingPageViewModel>();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.ParmDataGridCollection, v => v.ParmDataGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel,vm=>vm.SelectData,v=>v.ParmDataGrid.SelectedItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ParmSetCommand, v => v.ParmSetButton).DisposeWith(d);
            });
        }

        #region ViewModel
        public ParmValueSettingPageViewModel ViewModel
        {
            get { return (ParmValueSettingPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (ParmValueSettingPageViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(ParmValueSettingPageViewModel), typeof(ParmValueSettingPage), new PropertyMetadata(null));
        #endregion
    }
}
