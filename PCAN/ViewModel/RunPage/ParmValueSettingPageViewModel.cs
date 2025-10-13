using DynamicData;
using DynamicData.Binding;
using PCAN.Modles;
using PCAN.View.Windows;
using PCAN.ViewModel.Window;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PCAN.ViewModel.RunPage
{
    public class ParmValueSettingPageViewModel:ReactiveObject
    {
        public ParmValueSettingPageViewModel()
        {
            ParmDataGridSource
                .Connect() 
                .Sort(SortExpressionComparer<PCanParmDataGrid>.Ascending(x => x.ID)) // 排序
                .Sort(SortExpressionComparer<PCanParmDataGrid>.Ascending(x => x.Index)) // 排序
                .Bind(out _parmDataGridItems) 
                .Subscribe();
            ParmSetCommand = ReactiveCommand.Create(() =>
            {
                var windowviewmodle = new ParmValueSettingWindowViewModel(ParmDataGridSource, null);
                var window = new ParmValueSettingWindow(windowviewmodle);
                window.ShowDialog();
            });
            ParmDeleteCommand = ReactiveCommand.Create(() =>
            {
                if (SelectData!=null)
                {
                    ParmDataGridSource.Remove(SelectData);
                }
            });
            ParmEditCommand = ReactiveCommand.Create(() =>
            {
                if (SelectData!=null)
                {
                    var windowviewmodle = new ParmValueSettingWindowViewModel(ParmDataGridSource, SelectData);
                    var window = new ParmValueSettingWindow(windowviewmodle);
                    window.ShowDialog();
                }
                
            });
        }
        public ReactiveCommand<Unit, Unit> ParmSetCommand { get; }
        public ReactiveCommand<Unit,Unit> ParmDeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> ParmEditCommand { get; }

        [Reactive]
        public PCanParmDataGrid SelectData { get; set; }
        public SourceList<PCanParmDataGrid> ParmDataGridSource { get; set; }=new SourceList<PCanParmDataGrid>();
        private readonly ReadOnlyObservableCollection<PCanParmDataGrid> _parmDataGridItems;
        public ReadOnlyObservableCollection<PCanParmDataGrid> ParmDataGridCollection => _parmDataGridItems;
    }
}
