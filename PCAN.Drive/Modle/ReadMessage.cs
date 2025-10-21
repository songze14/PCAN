using Peak.Can.Basic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PCAN.Drive.Modle
{
    public class ReadMessage : ReactiveObject
    {
        public int ID { get; set; }
        [Reactive]
        public MessageType MSGTYPE { get; set; }
        public byte LEN { get; set; }
        private byte[] _DATA;

        public byte[] DATA { 
            get=> _DATA;
            set
            {
                this.RaiseAndSetIfChanged(ref _DATA, value);
                if (value != null)
                {
                    DATASTR = BitConverter.ToString(value);
                }
                
            }
        }
        [Reactive]
        public string DATASTR { get; set; }
        
        [Reactive]

        public double TimeStamp { get; set; }
        [Reactive]
        public int Count { get; set; } 
    }
}
