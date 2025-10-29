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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
                try
                {
                    if (SelectTypeInfo==null)
                    {
                        MessageBox.Show($"未选择参数类型！");

                        return;
                    }
                    if (PCanParmData == null)
                    {

                        ShowPCanParmData.Size = Marshal.SizeOf(SelectTypeInfo.TargetType);
                        ShowPCanParmData.Index = PCanParmDataGrids.Count + 1;
                        ShowPCanParmData.TargetType = SelectTypeInfo.FullName;
                        ShowPCanParmData.TargetFullName = SelectTypeInfo.Name;
                        PCanParmDataGrids.Add(ShowPCanParmData);
                    }
                    else
                    {
                        PCanParmDataGrids.Remove(PCanParmData);
                        PCanParmData.Name = ShowPCanParmData.Name;

                        PCanParmData.Size = Marshal.SizeOf(SelectTypeInfo.TargetType);
                        PCanParmData.StatrtIndex = ShowPCanParmData.StatrtIndex;
                        PCanParmData.EndIndex = ShowPCanParmData.EndIndex;

                        PCanParmData.TargetType = SelectTypeInfo.FullName;
                        PCanParmData.TargetFullName = SelectTypeInfo.Name;
                        PCanParmDataGrids.Add(PCanParmData);

                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"保存参数时出现错误:{ex.Message}");
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
