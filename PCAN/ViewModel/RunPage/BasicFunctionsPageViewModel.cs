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

namespace PCAN.ViewModel.RunPage
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
           
        }
      
       
        public string Title { get; set; } = "PCAN";
      
     
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
    }
}
