using MediatR;
using PCAN.Shard.Models;
using PCAN.ViewModel.RunPage;
using PCAN.ViewModel.USercontrols;
using PCAN.ViewModel.Window;

namespace PCAN.Notification.Log
{
     public class LogNotificationHandle : INotificationHandler<LogNotification>
     {
        private readonly UILogsViewModel _viewmodel;

        public LogNotificationHandle(UILogsViewModel viewModel)
        {
            _viewmodel = viewModel;
        }
        public Task Handle(LogNotification notification, CancellationToken cancellationToken)
        {
            var msg = notification;
            if (msg != null)
            {
                this._viewmodel.OnNext(new LogMessage() { Content=msg.Message,
                    EventSource=msg.LogSource.ToString(),
                    EventGroup= msg.LogSource.ToString(), 
                    Timestamp=DateTime.Now,
                    Level= msg.LogLevel
                });
            }
            return Task.CompletedTask;
        }
     }
}
