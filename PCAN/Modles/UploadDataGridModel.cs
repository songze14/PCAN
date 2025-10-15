using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Modles
{
    public class UploadDataGridModel:ReactiveObject
    {
        public int Index { get; set; }
        public byte[] Data { get; set; }
        public string Size { get; set; } = string.Empty;
        public byte CRC { get; set; }
        [Reactive]
        public int SendCount { get; set; }
        [Reactive]
        public bool IsOver { get; set; }
    }
}
