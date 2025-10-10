using PCAN.Notification.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Modles
{
    public class WindowLog
    {
        public DateTime DateTime { get; set; }
        public LogSource LogSource { get; set; }
        public string LogMessage { get; set; }
    }
}
