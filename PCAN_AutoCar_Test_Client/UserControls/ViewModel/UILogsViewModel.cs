using DynamicData;
using Microsoft.Extensions.Logging;
using PCAN.Shard.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace PCAN_AutoCar_Test_Client.ViewModel.USercontrols
{
    public class UILogsViewModel : ReactiveObject, IDisposable
    {
        private IDisposable _cleanup;
        public UILogsViewModel(ILogger<UILogsViewModel> logger)
        {
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
            var disposeCmdClear = this.CmdClear.ThrownExceptions.Subscribe(x => {
            });


            var eventgroupFilter = this.WhenAnyValue(x => x.EventGroup)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .DistinctUntilChanged()
                .Select(x => {
                    Func<LogMessage, bool> res = lm => {
                        if (string.IsNullOrEmpty(x))
                        {
                            return true;
                        }
                        return lm.EventGroup == x;
                    };
                    return res;
                });

            this.ChangeObs = this._source.Connect()
                .Filter(eventgroupFilter);

            var d = this.ChangeObs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _logs)
                .DisposeMany()
                .Subscribe();

            this._cleanup = new CompositeDisposable(
                d,
                disposeCmdClearFilterException,
                disposeCmdClear
            );
            this.logger = logger;
        }

        private SourceList<LogMessage> _source = new SourceList<LogMessage>();

        #region
        private readonly ReadOnlyObservableCollection<LogMessage> _logs;
        private readonly ILogger<UILogsViewModel> logger;

        public ReadOnlyObservableCollection<LogMessage> Logs => _logs;
        public IObservable<IChangeSet<LogMessage>> ChangeObs { get; }
        #endregion

        #region
        [Reactive]
        public string EventGroup { get; set; }
        #endregion

        public void OnNext(LogMessage msg)
        {
            while (this._source.Count > 1000)
            {
                this._source.RemoveAt(0);
            }
            this._source.Add(msg);
            switch (msg.Level)
            {
                case LogLevel.Trace:
                    logger.LogTrace($"{msg.Content}");
                    break;

                case LogLevel.Debug:
                    logger.LogDebug($"{msg.Content}");
                    break;

                case LogLevel.Information:
                    logger.LogInformation($"{msg.Content}");
                    break;

                case LogLevel.Warning:
                    logger.LogWarning($"{msg.Content}");
                    break;
                case LogLevel.Error:
                    logger.LogError($"{msg.Content}");
                    break;
                case LogLevel.Critical:
                    logger.LogCritical($"{msg.Content}");
                    break;
                case LogLevel.None:
                    logger.LogInformation($"{msg.Content}");

                    break;
                default:
                    break;
            }
        }

        public ReactiveCommand<Unit, Unit> CmdClearFilter { get; }
        public ReactiveCommand<Unit, Unit> CmdClear { get; }

        public void Dispose()
        {
            this._cleanup.Dispose();
        }
    }
}
