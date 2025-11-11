using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.UserControls;
using PCAN.ViewModel.USercontrols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PCAN.ViewModel.RunPage
{
    public class DataMonitoringPageViewModel: ReactiveObject
    {
        private readonly ILogger<BasicFunctionsPageViewModel> _logger;
        private readonly IMediator _mediator;

        public DataMonitoringPageViewModel(WpfPlotGLUserControl wpfPlotGLUserControl, ILogger<BasicFunctionsPageViewModel> logger, IMediator mediator, PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel)
        {
            WpfPlotGLUserControl = wpfPlotGLUserControl;
            _logger = logger;
            _mediator = mediator;
            PCanClientUsercontrolViewModel = pCanClientUsercontrolViewModel;
        }
        [Reactive]
        public WpfPlotGLUserControl WpfPlotGLUserControl { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; set; }
    }
}
