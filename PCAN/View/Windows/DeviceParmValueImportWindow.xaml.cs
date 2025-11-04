using PCAN.ViewModel.Window;
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
using System.Windows.Shapes;

namespace PCAN.View.Windows
{
    /// <summary>
    /// DeviceParmValueImportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceParmValueImportWindow : Window,IViewFor<DeviceParmValueImportWindowViewModel>
    {
        public DeviceParmValueImportWindow()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.InputParmStr, v => v.DeviceParmValueStrTextBox.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ParseCommand, v => v.ParseButton).DisposeWith(d);
            });
        }
        #region ViewModel
        public DeviceParmValueImportWindowViewModel ViewModel
        {
            get { return (DeviceParmValueImportWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (DeviceParmValueImportWindowViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DeviceParmValueImportWindowViewModel), typeof(DeviceParmValueImportWindow), new PropertyMetadata(null));
        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
