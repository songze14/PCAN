using PCAN.View.RealtimePage;
using PCAN.ViewModel.Window;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
    /// ParmValueSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceParmValueSettingWindow : Window,IViewFor<DeviceParmValueSettingWindowViewModel>
    {
        public DeviceParmValueSettingWindow(DeviceParmValueSettingWindowViewModel viewModle)
        {
            InitializeComponent();
            this.ViewModel = viewModle;
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.ShowPCanParmData.ID, v => v.ID.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IDReadOnlay, v => v.ID.IsReadOnly).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShowPCanParmData.Name, v => v.Name.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShowPCanParmData.Size, v => v.Size.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShowPCanParmData.StatrtIndex, v => v.StatrtIndex.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShowPCanParmData.EndIndex, v => v.EndIndex.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShowPCanParmData.DataStatrtIndex, v => v.DataStatrtIndex.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShowPCanParmData.DataEndIndex, v => v.DataEndIndex.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveCommand, v => v.SaveButton).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TypeInfos, v => v.TargetType.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectTypeInfo, v => v.TargetType.SelectedItem).DisposeWith(d);
            });
        }

        #region ViewModel
        public DeviceParmValueSettingWindowViewModel ViewModel
        {
            get { return (DeviceParmValueSettingWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (DeviceParmValueSettingWindowViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DeviceParmValueSettingWindowViewModel), typeof(DeviceParmValueSettingWindow), new PropertyMetadata(null));
        #endregion

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.SaveCommand.Execute().Subscribe();
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
