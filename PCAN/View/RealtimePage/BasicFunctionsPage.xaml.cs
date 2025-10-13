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

namespace PCAN.View.UserPage
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class BasicFunctionsPage : Page, IViewFor<BasicFunctionsPageViewModel>
    {
        public BasicFunctionsPage()
        {
            this.ViewModel = Locator.Current.GetService<BasicFunctionsPageViewModel>();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Title, v => v.Title).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PCanClientUsercontrolViewModel, v => v.PcanClientUsercontrol.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PCanClientUsercontrolViewModel.TPCANMsgs, v => v.CanMessageDataGrid.ItemsSource).DisposeWith(d);


            });
            InitializeComponent();

        }
        #region ViewModel
        public BasicFunctionsPageViewModel ViewModel
        {
            get { return (BasicFunctionsPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (BasicFunctionsPageViewModel)value; }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(BasicFunctionsPageViewModel), typeof(BasicFunctionsPage), new PropertyMetadata(null));
        #endregion

    }
}
