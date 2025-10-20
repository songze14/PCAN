using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PCAN_AutoCar_Test_Client.ViewModel
{
    public class MainWindowViewModel : ReactiveObject
    {
        public MainWindowViewModel(UILogsViewModel uILogsViewModel)
        {
            UILogsViewModel = uILogsViewModel;
        }

        public ICommand AddLogCommand { get; }
        public UILogsViewModel UILogsViewModel { get; }
    }
}
