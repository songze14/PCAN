using Peak.Can.Basic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PCAN.Drive.Modle
{
    public class ReadMessage : ReactiveObject
    {
        public int ID { get; set; }
        [Reactive]
        public TPCANMessageType MSGTYPE { get; set; }
        public byte LEN { get; set; }
        [Reactive]

        public string DATA { get; set; }
        [Reactive]

        public double TimeStamp { get; set; }
        [Reactive]
        public int Count { get; set; } 
    }
}
