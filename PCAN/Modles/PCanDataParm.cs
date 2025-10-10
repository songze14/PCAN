using PCAN.View.Windows;
using PCAN.ViewModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Modles
{
    public class PCanParm
    {
        public int ID { get; set; }
        public List<PCanDataParm> PCanDataParms { get; set; }

    }
    public class  PCanDataParm
    {
        public string Name { get; set; }
        public ushort Size { get; set; }
        public ushort StatrtIndex { get; set; }
        public ushort EndIndex { get; set; }
    }
    public class PCanParmDataGrid
    {
        public PCanParmDataGrid()
        {
            
        }
        [Reactive]
        public int ID { get; set; }
        public int Index { get; set; }
        [Reactive]
        public string Name { get; set; }
        [Reactive]
        public ushort Size { get; set; }
        [Reactive]
        public ushort StatrtIndex { get; set; }
        [Reactive]
        public ushort EndIndex { get; set; }
    }
}
