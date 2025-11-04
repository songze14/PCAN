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
        [Reactive]
        public int ID { get; set; }
        public int Index { get; set; }
        [Reactive]
        public string Name { get; set; }
        [Reactive]
        public int Size { get; set; }
        /// <summary>
        /// 解析起始位
        /// </summary>
        [Reactive]
        public ushort StatrtIndex { get; set; }
        /// <summary>
        /// 解析结束位
        /// </summary>
        [Reactive]
        public ushort EndIndex { get; set; }
    }
    public class DevicePCanParmDataGrid : PCanParmDataGrid
    {
        [Reactive]
        public string TargetType { get; set; }
        public string TargetFullName { get; set; }
        ///// <summary>
        ///// 数据起始位
        ///// </summary>
        //[Reactive]
        //public ushort DataStatrtIndex { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Reactive]
        public string Remark { get; set; }
        [Reactive]
        public string Value { get; set; }
        [Reactive]
        public string SetValue { get; set; }

    }
    public class TypeInfo
    {
        public string Name { get; set; }
        public Type TargetType { get; set; }
        public string FullName { get; set; }
        public int Size { get; set; }   
    }
}
