using PCAN_AutoCar_Test_Client.ViewModel;
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

namespace PCAN_AutoCar_Test_Client.View
{
    /// <summary>
    /// TestRealtimePage.xaml 的交互逻辑
    /// </summary>
    public partial class TestRealtimePage : Page,IViewFor<TestRealtimePageViewModel>
    {
        public TestRealtimePage()
        {
            InitializeComponent();
            ViewModel=Locator.Current.GetService<TestRealtimePageViewModel>();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.PCanClientUsercontrolViewModel, v => v.pcanClientUsercontrol.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TestExcelGridModels, v => v.TestExcelDataGrid.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SelectedFilePath, v => v.FilePathTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TestExcelGridModels, v => v.TestExcelDataGrid.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CanStartTesta, v => v.BeTestButton.IsEnabled).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.DebugTesta, v => v.DebugCheckBox.IsChecked).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.BrowseFileCommand, v => v.BrowseFileButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.TestCommand, v => v.BeTestButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StepTestCommand, v => v.DebugButton).DisposeWith(d);
                this.BindCommand(ViewModel,vm=>vm.StopTestCommand,v=>v.DebugStopButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ExportTemplateCommand, v => v.ExTestButton).DisposeWith(d);

            });
        }
        #region ViewModel
        public TestRealtimePageViewModel ViewModel
        {
            get { return (TestRealtimePageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (TestRealtimePageViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(TestRealtimePageViewModel), typeof(TestRealtimePage), new PropertyMetadata(null));
        #endregion
    }
}
