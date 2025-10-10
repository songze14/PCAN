using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.Drive;
using PCAN.Drive.Modle;
using PCAN.Modles;
using PCAN.Notification.Log;
using PCAN.ViewModel.USercontrols;
using Peak.Can.Basic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace PCAN.ViewModel
{
    public class BasicFunctionsPageViewModel : ReactiveObject
    {
        private readonly ILogger<BasicFunctionsPageViewModel> _logger;
        private readonly IMediator _mediator;

        public BasicFunctionsPageViewModel(ILogger<BasicFunctionsPageViewModel> logger,IMediator mediator, PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel)
        {
            _logger = logger;
            _mediator = mediator;
            PCanClientUsercontrolViewModel = pCanClientUsercontrolViewModel;
            this.AddLogCommand = ReactiveCommand.Create<LogNotification>(log =>
            {
                if (WindowLogs.Count >= 100)
                {
                    WindowLogs.RemoveAt(0);
                }
                WindowLogs.Add(new WindowLog()
                {
                    DateTime = DateTime.Now,
                    LogMessage = log.Message,
                    LogSource = log.LogSource
                });
            });
           
        }
      
       
        public string Title { get; set; } = "PCAN";
      
        public ObservableCollection<WindowLog> WindowLogs { get; set; } = [];
     
        public ICommand AddLogCommand { get; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
    }
}
