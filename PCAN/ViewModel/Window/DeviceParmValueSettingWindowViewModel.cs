using DynamicData;
using PCAN.Modles;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.ViewModel.Window
{
    public class DeviceParmValueSettingWindowViewModel : ReactiveObject
    {
        public DeviceParmValueSettingWindowViewModel(ObservableCollection<TypeInfo> typeInfos, SourceList<DevicePCanParmDataGrid> pCanParmDataGrids, DevicePCanParmDataGrid? pCanParmData)
        {
            TypeInfos = typeInfos;
            PCanParmDataGrids = pCanParmDataGrids;
            if (pCanParmData==null)
            {
                IDReadOnlay = false;
                SelectTypeInfo = typeInfos[0];
            }
            else
            {
                ShowPCanParmData = PCanParmData = pCanParmData;
                SelectTypeInfo=typeInfos.FirstOrDefault(t => t.FullName == PCanParmData.TargetType);
            }
            SaveCommand = ReactiveCommand.Create(() =>
            {
                if (PCanParmData==null)
                {
                    ShowPCanParmData.Index = PCanParmDataGrids.Count+1;
                    ShowPCanParmData.TargetType = SelectTypeInfo?.FullName ?? string.Empty;
                    ShowPCanParmData.TargetFullName = SelectTypeInfo.Name ?? string.Empty;
                    PCanParmDataGrids.Add(ShowPCanParmData);
                }
                else
                {
                    //PCanParmDataGrids.Remove(PCanParmData);
                    PCanParmData.Name = ShowPCanParmData.Name;
                    PCanParmData.DataEndIndex = ShowPCanParmData.DataEndIndex;
                    PCanParmData.DataStatrtIndex = ShowPCanParmData.DataStatrtIndex;
                    PCanParmData.Size =(ushort)(ShowPCanParmData.DataEndIndex - ShowPCanParmData.DataStatrtIndex);
                    PCanParmData.StatrtIndex = ShowPCanParmData.StatrtIndex;
                    PCanParmData.EndIndex = ShowPCanParmData.EndIndex;
                   
                    PCanParmData.TargetType = SelectTypeInfo?.FullName??string.Empty;
                    PCanParmData.TargetFullName=SelectTypeInfo.Name??string.Empty;
                    //PCanParmDataGrids.Add(PCanParmData);

                }
            });
        }
        [Reactive]
        public bool IDReadOnlay { get; set; }
        public ReactiveCommand<Unit,Unit> SaveCommand { get; }
        [Reactive]
        public DevicePCanParmDataGrid ShowPCanParmData { get; set; } = new DevicePCanParmDataGrid();
        public ObservableCollection<TypeInfo> TypeInfos { get; }
        [Reactive]
        public TypeInfo? SelectTypeInfo { get; set; }
        public SourceList<DevicePCanParmDataGrid> PCanParmDataGrids { get; }
        public DevicePCanParmDataGrid? PCanParmData { get; }
    }
}
