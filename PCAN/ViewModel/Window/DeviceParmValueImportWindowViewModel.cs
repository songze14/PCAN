using DynamicData;
using PCAN.Modles;
using PCAN.Shard.Tools;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace PCAN.ViewModel.Window
{
    public partial class DeviceParmValueImportWindowViewModel:ReactiveObject
    {
        public DeviceParmValueImportWindowViewModel(SourceList<DevicePCanParmDataGrid> sourceList)
        {
            ParseCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    sourceList.Clear();
                    if (string.IsNullOrWhiteSpace(InputParmStr))
                    {
                        return;
                    }
                    var inputparmstrs = InputParmStr.Split("\r\n");
                    var typerepagex = ParmRegex.TypeRegex();
                    var namerepagex = ParmRegex.NameRegex();
                    var remarkrepagex = ParmRegex.RemarkRegex();
                    var remark = string.Empty;
                    foreach (var inputparmstr in inputparmstrs)
                    {
                        var typematch = typerepagex.Match(inputparmstr);
                        if (typematch == null || !typematch.Success)
                        {
                            MessageBox.Show($"字符串{inputparmstr}找不到数据类型！");
                            return;
                        }
                        var type = typematch.Value;
                        var typeinfo = CTypeToCsharpTypeValue.TypeInfos.FirstOrDefault(o => o.Name == type);
                        if (typeinfo == null)
                        {
                            MessageBox.Show($"解析参数失败:{inputparmstr}存在不可解析类型{type}");
                            return;
                        }
                        var namematch = namerepagex.Match(inputparmstr);
                        if (namematch == null || !namematch.Success)
                        {
                            MessageBox.Show($"字符串{inputparmstr}找不到参数名称！");
                            return;
                        }
                        var name = namematch.Value;
                        var remarkmatch = remarkrepagex.Match(inputparmstr);
                        if (remarkmatch != null && remarkmatch.Success)
                        {
                            remark = remarkmatch.Value;
                        }
                        else
                        {
                            remark = string.Empty;
                        }
                        sourceList.Add(new DevicePCanParmDataGrid()
                        {
                            ID = sourceList.Count + 1,
                            Index = sourceList.Count,
                            Name = name,
                            Remark = remark,
                            Size = typeinfo.Size,
                            TargetFullName = typeinfo.Name,
                            TargetType = typeinfo.FullName,
                        });
                    }
                    MessageBox.Show("解析完成");

                }
                catch (Exception ex)
                {

                    MessageBox.Show($"解析参数失败:{ex.Message}");
                }
                

            });
        }
        [Reactive]
        public string InputParmStr { get; set; }
       
        public ReactiveCommand<Unit,Unit> ParseCommand { get; set; }
       
            
        
        public ObservableCollection<TypeInfo> TypeInfos { get; set; } =
        [
            new TypeInfo(){Name="u8",TargetType=typeof(byte),FullName=typeof(byte).FullName,Size=Marshal.SizeOf(typeof(byte))},
            new TypeInfo(){Name="u16",TargetType=typeof(ushort),FullName=typeof(ushort).FullName,Size=Marshal.SizeOf(typeof(ushort))},
            new TypeInfo(){Name="u32",TargetType=typeof(uint),FullName=typeof(uint).FullName,Size=Marshal.SizeOf(typeof(uint))},
            new TypeInfo(){Name="u64",TargetType=typeof(ulong),FullName=typeof(ulong).FullName,Size=Marshal.SizeOf(typeof(ulong))},
            new TypeInfo(){Name="s8",TargetType=typeof(sbyte),FullName=typeof(sbyte).FullName,Size=Marshal.SizeOf(typeof(sbyte))},
            new TypeInfo(){Name="s16",TargetType=typeof(short),FullName=typeof(short).FullName,Size=Marshal.SizeOf(typeof(short))},
            new TypeInfo(){Name="s32",TargetType=typeof(int),FullName=typeof(int).FullName,Size=Marshal.SizeOf(typeof(int))},
            new TypeInfo(){Name="s64",TargetType=typeof(long),FullName=typeof(long).FullName,Size=Marshal.SizeOf(typeof(long))},
            new TypeInfo(){Name="float",TargetType=typeof(float),FullName=typeof(float).FullName,Size=Marshal.SizeOf(typeof(float))},
            new TypeInfo(){Name="char",TargetType=typeof(char),FullName=typeof(char).FullName,Size=Marshal.SizeOf(typeof(char))},
        ];
    }
}
