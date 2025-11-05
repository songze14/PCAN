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
    /// DeviceParmTuningPage.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceParmTuningPage : Page,IViewFor<DeviceParmTuningPageViewModel>
    {
        public DeviceParmTuningPage()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<DeviceParmTuningPageViewModel>();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.PCanClientUsercontrolViewModel, v => v.PcanClientUsercontrol.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SelectedFilePath, v => v.ParmFilePathTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ParmDataGridCollection, v => v.ParmDataGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectData, v => v.ParmDataGrid.SelectedItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ParmAddCommand, v => v.ParmAddButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.BrowseFileCommand, v => v.BrowseFileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveParmFileCommand, v => v.SaveParmFileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.LoadParmFileCommand, v => v.LoadParmFileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ReadParmCommand, v => v.ReadParmButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenImportParmWindowCommand, v => v.ImportParmButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SendParmCommand, v => v.SendParmButton).DisposeWith(d);
            });
        }
        #region ViewModel
        public DeviceParmTuningPageViewModel ViewModel
        {
            get { return (DeviceParmTuningPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (DeviceParmTuningPageViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DeviceParmTuningPageViewModel), typeof(DeviceParmTuningPage), new PropertyMetadata(null));
        #endregion
    }
}
