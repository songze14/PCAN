using PCAN.Shard.Models;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
using ReactiveUI;
using Splat;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PCAN_AutoCar_Test_Client.UserControls
{
    /// <summary>
    /// UILogsView.xaml 的交互逻辑
    /// </summary>
    public partial class UILogsView : UserControl, IViewFor<UILogsViewModel>
    {
        public UILogsView()
        {
            InitializeComponent();
            this.ViewModel= Locator.Current.GetService<UILogsViewModel>();
            this.WhenActivated(d => {
                this.OneWayBind(this.ViewModel, vm => vm.Logs, v => v.logs.ItemsSource).DisposeWith(d);
                this.Bind(this.ViewModel, vm => vm.EventGroup, v => v.logsEventGroup.Text).DisposeWith(d);

                this.BindCommand(this.ViewModel, vm => vm.CmdClearFilter, v => v.clearFilter).DisposeWith(d);

                this.BindCommand(this.ViewModel, vm => vm.CmdClear, v => v.clear, nameof(this.clear.Click)).DisposeWith(d);

                this.ViewModel?.ChangeObs
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(lm => {
                        this.scroll.ScrollToEnd();
                    })
                    .DisposeWith(d);
            });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuitem = sender as MenuItem;
            var logmsg = menuitem.DataContext as LogMessage;
            this.logsEventGroup.Text = logmsg.EventGroup;
        }

        #region ViewModel
        public UILogsViewModel ViewModel
        {
            get { return (UILogsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (UILogsViewModel)value; }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(UILogsViewModel), typeof(UILogsView), new PropertyMetadata(null));
        #endregion
    }
}
