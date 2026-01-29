using DynamicData;
using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.Drive;
using PCAN.Drive.Modle;
using PCAN.Modles;
using PCAN.Notification.Log;
using PCAN.Shard.Models;
using PCAN.Tools;
using PCAN.ViewModel.USercontrols;
using Peak.Can.Basic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using Unit = System.Reactive.Unit;

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
            this.PCanClientUsercontrolViewModel.NewMessage.Subscribe(msg =>
            {
                if (msg==null)
                {
                    return;
                }
                UIHelper.RunInUIThread(d =>
                {
                    var oldmsg = TPCANMsgs.FirstOrDefault(x => x.ID == msg.ID);
                 
                    if (oldmsg != null)
                    {
                        oldmsg.MSGTYPE = msg.MSGTYPE;
                        oldmsg.LEN = msg.LEN;
                        oldmsg.DATA = msg.DATA;
                        oldmsg.Count++;
                    }
                    else
                    {
                        _source.Add(msg);
                    }
                });


            });

            var eventgroupFilter = this.WhenAnyValue(x => x.EventGroup)
               .Throttle(TimeSpan.FromMilliseconds(400))
               .DistinctUntilChanged()
               .Select(x => {
                   Func<ReadMessage, bool> res = lm => {
                       if (string.IsNullOrEmpty(x))
                       {
                           return true;
                       }
                       var id = Convert.ToUInt32(x, 16);
                       return lm.ID == id;
                   };
                   return res;
               });

            this.ChangeObs = this._source.Connect()
                .Filter(eventgroupFilter);

            var d = this.ChangeObs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _TPCANMsgs)
                .DisposeMany()
                .Subscribe();

            this.CmdClearFilter = ReactiveCommand.Create(() =>
            {
                this.EventGroup = "";
            });

            var disposeCmdClearFilterException = this.CmdClearFilter.ThrownExceptions.Subscribe(x => {
                
            });
            this.CmdClear = ReactiveCommand.Create(() =>
            {
                this._source.Clear();
            });
        }
        private bool Filting;
        [Reactive]
        public string EventGroup { get; set; } 
        private SourceList<ReadMessage> _source = new SourceList<ReadMessage>();
        private readonly ReadOnlyObservableCollection<ReadMessage> _TPCANMsgs;
        public ReadOnlyObservableCollection<ReadMessage> TPCANMsgs => _TPCANMsgs;

        public IObservable<IChangeSet<ReadMessage>> ChangeObs { get; }

        public string Title { get; set; } = "PCAN";
        public ReactiveCommand<Unit, Unit> CmdClearFilter { get; }
        public ReactiveCommand<Unit, Unit> CmdClear { get; }


        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
    }
}
