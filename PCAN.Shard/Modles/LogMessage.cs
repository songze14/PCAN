using Microsoft.Extensions.Logging;
using PCAN.Notification.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Shard.Models
{
    public class LogMessage
    {
        public string EventSource { get; set; }

        public string EventGroup { get; set; } = "";


        public DateTime Timestamp { get; set; }

        public LogLevel Level { get; set; }

        public string Content { get; set; }
    }
}
