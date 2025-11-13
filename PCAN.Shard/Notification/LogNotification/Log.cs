using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Notification.Log
{
    
    public enum LogSource
    {
        CanDevice,
        Upload,
        DeviceParm,
        TestRealtime,
        DataMonitoring
    }
    public class LogNotification : INotification
    {
        public LogLevel LogLevel { get; set; }
        public LogSource  LogSource { get; set; }
        public string Message { get; set; }
    }
}
