using DynamicData;
using DynamicData.Binding;
using MediatR;
using PCAN.Modles;
using PCAN.View.Windows;
using PCAN.ViewModel.Window;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unit = System.Reactive.Unit;

namespace PCAN.ViewModel.RunPage
{
    public class DeviceParmTuningPageViewModel:ReactiveObject
    {
        private readonly IMediator _mediator;

        public DeviceParmTuningPageViewModel(IMediator mediator)
        {
            _mediator = mediator;
            ParmDataGridSource
               .Connect()
               .Sort(SortExpressionComparer<PCanParmDataGrid>.Ascending(x => x.ID)) // 排序
               .Sort(SortExpressionComparer<PCanParmDataGrid>.Ascending(x => x.Index)) // 排序
               .Bind(out _parmDataGridItems)
               .Subscribe();
            ParmAddCommand = ReactiveCommand.Create(() =>
            {
                var windowviewmodle = new DeviceParmValueSettingWindowViewModel(TypeInfos, ParmDataGridSource, null);
                var window = new DeviceParmValueSettingWindow(windowviewmodle);
                window.ShowDialog();
            });
            ParmDeleteCommand = ReactiveCommand.Create(() =>
            {
                if (SelectData != null)
                {
                    ParmDataGridSource.Remove(SelectData);
                }
            });
            ParmEditCommand = ReactiveCommand.Create(() =>
            {
                if (SelectData != null)
                {
                   
                    var windowviewmodle = new DeviceParmValueSettingWindowViewModel(TypeInfos, ParmDataGridSource, SelectData);
                    var window = new DeviceParmValueSettingWindow(windowviewmodle);
                    window.ShowDialog();
                }

            });
        }
        public ReactiveCommand<Unit, Unit> ParmAddCommand { get; }
        public ReactiveCommand<Unit, Unit> ParmDeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> ParmEditCommand { get; }

        [Reactive]
        public DevicePCanParmDataGrid SelectData { get; set; }
        public SourceList<DevicePCanParmDataGrid> ParmDataGridSource { get; set; } = new SourceList<DevicePCanParmDataGrid>();
        private readonly ReadOnlyObservableCollection<DevicePCanParmDataGrid> _parmDataGridItems;
        public ReadOnlyObservableCollection<DevicePCanParmDataGrid> ParmDataGridCollection => _parmDataGridItems;
        public  ObservableCollection<TypeInfo> TypeInfos { get; set; }=
        [
            new TypeInfo(){Name="Byte",TargetType=typeof(byte),FullName=typeof(byte).FullName},
            new TypeInfo(){Name="Int16",TargetType=typeof(short),FullName=typeof(short).FullName},
            new TypeInfo(){Name="UInt16",TargetType=typeof(ushort),FullName=typeof(ushort).FullName},
            new TypeInfo(){Name="Int32",TargetType=typeof(int),FullName=typeof(int).FullName},
            new TypeInfo(){Name="UInt32",TargetType=typeof(uint),FullName=typeof(uint).FullName},
            new TypeInfo(){Name="Int64",TargetType=typeof(long),FullName=typeof(long).FullName},
            new TypeInfo(){Name="UInt64",TargetType=typeof(ulong),FullName=typeof(ulong).FullName},
            new TypeInfo(){Name="Single",TargetType=typeof(float),FullName=typeof(float).FullName},
            new TypeInfo(){Name="Double",TargetType=typeof(double),FullName=typeof(double).FullName},
            new TypeInfo(){Name="Boolean",TargetType=typeof(bool),FullName=typeof(bool).FullName},
        ];
    }
}
