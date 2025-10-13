using PCAN.Modles;
using PCAN.Notification.Log;
using PCAN.ViewModel.USercontrols;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PCAN.ViewModel.Window
{
    public class MainWindowViewModel:ReactiveObject
    {
        public MainWindowViewModel(UILogsViewModel uILogsViewModel)
        {
            UILogsViewModel = uILogsViewModel;
        }
       
        public ICommand AddLogCommand { get; }
        public UILogsViewModel UILogsViewModel { get; }
    }
}
