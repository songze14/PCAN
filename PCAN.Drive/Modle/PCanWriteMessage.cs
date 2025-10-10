using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Drive.Modle
{
    public class PCanWriteMessage
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
    }
}
