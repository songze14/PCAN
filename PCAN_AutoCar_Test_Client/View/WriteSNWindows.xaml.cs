using PCAN_AutoCar_Test_Client.ViewModel;
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

namespace PCAN_AutoCar_Test_Client.View
{
    /// <summary>
    /// WriteSNWindows.xaml 的交互逻辑
    /// </summary>
    public partial class WriteSNWindows : Window,IViewFor<WriteSNWindowsViewModel>
    {
        public WriteSNWindows()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.SN, v => v.SNTextBox.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.WriteCommand, v =>v.WriteButton).DisposeWith(d);
            });
            this.SNTextBox.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region ViewModel
        public WriteSNWindowsViewModel ViewModel
        {
            get { return (WriteSNWindowsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (WriteSNWindowsViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(WriteSNWindowsViewModel), typeof(WriteSNWindows), new PropertyMetadata(null));
        #endregion
    }
}
