using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Drive.Modle
{
    public class PCanWriteMessage
    {
        public uint Id { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Standard;
        public byte[] Data { get; set; }
    }
}
