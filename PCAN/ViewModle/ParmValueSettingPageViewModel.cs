using DynamicData;
using DynamicData.Binding;
using PCAN.Modles;
using PCAN.View.Windows;
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

namespace PCAN.ViewModle
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
            this.ParmSetCommand = ReactiveCommand.Create(() =>
            {
                var windowviewmodle = new ParmValueSettingWindowViewModle(ParmDataGridSource, null);
                var window = new ParmValueSettingWindow(windowviewmodle);
                window.ShowDialog();
            });
            this.ParmDeleteCommand = ReactiveCommand.Create(() =>
            {
                if (SelectData!=null)
                {
                    ParmDataGridSource.Remove(SelectData);
                }
            });
            this.ParmEditCommand = ReactiveCommand.Create(() =>
            {
                if (SelectData!=null)
                {
                    var windowviewmodle = new ParmValueSettingWindowViewModle(ParmDataGridSource, SelectData);
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
