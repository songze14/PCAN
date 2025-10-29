using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN_AutoCar_Test_Client.Modles
{
    public class UploadDataGridModel:ReactiveObject
    {
        public int Index { get; set; }
        public byte[] Data { get; set; }
        public string Size { get; set; } = string.Empty;
      
        [Reactive]
        public int SendCount { get; set; }
        [Reactive]
        public bool IsOver { get; set; }
    }
    public class UploadDevice
    {
        public string Name { get; set; }    
        public int Value { get; set; }
    }
}
