using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Shard.Models
{
    public class LocalPorts
    {
        public string PortName { get; set; }
        public ushort PortsNum { get; set; }
    }
    public class LocalBaudRate
    {
        public string BaudRateName { get; set; }
        public Bitrate Baudrate { get; set; }
    }
    public class LocalFDBaudRate
    {
        public string BaudRateName { get; set; }
        public string Baudrate { get; set; }
    }
    public class LocalDevice
    {
        public string DeviceID { get; set; }
        public Bitrate Baudrate { get; set; }
    }
}
