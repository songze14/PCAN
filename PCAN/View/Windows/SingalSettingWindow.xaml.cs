using MaterialDesignThemes.Wpf;
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
    /// SingalSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SignalSettingWindow : Window,IViewFor<SignalSettingWindowViewModel>
    {
        public SignalSettingWindow()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {

                this.Bind(ViewModel, vm => vm.XOFF, v => v.XOFFTextBox.Text,vmp=>vmp.ToString(), v => {
                    double num;
                    if (double.TryParse(v, out num))
                    {
                        return num;
                    }
                    return 0;
                }).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.YOFF, v => v.YOFFTextBox.Text,vmp=>vmp.ToString(), v => {
                    double num;
                    if (double.TryParse(v, out num))
                    {
                        return num;
                    }
                    return 0;
                }).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Gain, v => v.GainTextBox.Text,vmp=>vmp.ToString(),v=> { 
                    double num;
                    if (double.TryParse(v, out num))
                    {
                        return num;
                    }
                    return 0;
                    }).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Signals, v => v.SignalComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedSignal, v => v.SignalComboBox.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Color, v => v.signalColorPicker.Color).DisposeWith(d);
              
                this.BindCommand(ViewModel, vm => vm.XOFFChangeCommand, v => v.XOFFTextBox,vm=>vm.XOFF, nameof(TextBox.TextChanged)).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.YOFFChangeCommand, v => v.YOFFTextBox,vm=>vm.YOFF, nameof(TextBox.TextChanged)).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.GainChangeCommand, v => v.GainTextBox,vm=>vm.Gain ,nameof(TextBox.TextChanged)).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RestCommand, v => v.RestButton).DisposeWith(d);
            });
        }
        #region ViewModel
        public SignalSettingWindowViewModel ViewModel
        {
            get { return (SignalSettingWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (SignalSettingWindowViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(SignalSettingWindowViewModel), typeof(SignalSettingWindow), new PropertyMetadata(null));

        #endregion

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
