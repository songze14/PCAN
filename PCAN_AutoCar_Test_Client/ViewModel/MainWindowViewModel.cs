using Microsoft.Extensions.Options;
using PCAN.Shard.Tools;
using PCAN_AutoCar_Test_Client.Models;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            Version = "电控测试"+ VersionInfoHelper.GetShortVersion();
        }
        [Reactive]
        public string Title { get; set; }
        public ICommand AddLogCommand { get; }
        [Reactive]
        public string Version { get; set; }
        public UILogsViewModel UILogsViewModel { get; }
       
    }
}
