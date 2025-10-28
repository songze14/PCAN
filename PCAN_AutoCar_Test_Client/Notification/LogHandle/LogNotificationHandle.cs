using MediatR;
using PCAN.Notification.Log;
using PCAN.Shard.Models;
using PCAN_AutoCar_Test_Client.Tools;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;


namespace PCAN_AutoCar_Test_Client.Notification.Log
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
            UIHelper.RunInUIThread((a) =>
            {
                var msg = notification;
                if (msg != null)
                {
                    this._viewmodel.OnNext(new LogMessage()
                    {
                        Content = msg.Message,
                        EventSource = msg.LogSource.ToString(),
                        EventGroup = msg.LogSource.ToString(),
                        Timestamp = DateTime.Now,
                        Level = msg.LogLevel
                    });
                }
            });
           
            return Task.CompletedTask;
        }
     }
}
