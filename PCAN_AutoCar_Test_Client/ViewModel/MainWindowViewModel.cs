using Microsoft.Extensions.Options;
using PCAN_AutoCar_Test_Client.Models;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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
        private readonly AppSet _appset;

        public MainWindowViewModel(UILogsViewModel uILogsViewModel,IOptions<AppSet> appoptions)
        {
            UILogsViewModel = uILogsViewModel;
            _appset = appoptions.Value;
            Title = _appset.Title;
        }
        [Reactive]
        public string Title { get; set; }
        public ICommand AddLogCommand { get; }
        public UILogsViewModel UILogsViewModel { get; }
    }
}
