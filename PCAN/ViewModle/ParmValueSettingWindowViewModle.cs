using DynamicData;
using PCAN.Modles;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.ViewModle
{
    public class ParmValueSettingWindowViewModle:ReactiveObject
    {
        public ParmValueSettingWindowViewModle(SourceList<PCanParmDataGrid> pCanParmDataGrids, PCanParmDataGrid? pCanParmData)
        {
            PCanParmDataGrids = pCanParmDataGrids;
            if (pCanParmData==null)
            {
                IDReadOnlay = false;
            }
            else
            {
                ShowPCanParmData = PCanParmData = pCanParmData;
            }
            this.SaveCommand = ReactiveCommand.Create(() =>
            {
                if (PCanParmData==null)
                {
                    ShowPCanParmData.Index = PCanParmDataGrids.Count+1;
                    PCanParmDataGrids.Add(ShowPCanParmData);
                }
                else
                {
                    PCanParmDataGrids.Remove(PCanParmData);
                    PCanParmData.Name = ShowPCanParmData.Name;
                    PCanParmData.Size = ShowPCanParmData.Size;
                    PCanParmData.StatrtIndex = ShowPCanParmData.StatrtIndex;
                    PCanParmData.EndIndex = ShowPCanParmData.EndIndex;
                    PCanParmDataGrids.Add(PCanParmData);

                }
            });
        }
        [Reactive]
        public bool IDReadOnlay { get; set; }
        public ReactiveCommand<Unit,Unit> SaveCommand { get; }
        [Reactive]
        public PCanParmDataGrid ShowPCanParmData { get; set; } = new PCanParmDataGrid();
        public SourceList<PCanParmDataGrid> PCanParmDataGrids { get; }
        public PCanParmDataGrid? PCanParmData { get; }
    }
}
