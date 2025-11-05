using DynamicData;
using PCAN.Modles;
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
                    var repagex = ParmRegex();
                    var parmstr = string.Empty;
                    var remark = string.Empty;
                    foreach (var inputparmstr in inputparmstrs)
                    {
                        remark = string.Empty;
                        if (repagex.IsMatch(inputparmstr))
                        {
                            var parmandremarkstrs = inputparmstr.Split("//");
                            if (parmandremarkstrs.Length==2)
                            {
                                parmstr=parmandremarkstrs[0];
                                remark = parmandremarkstrs[1];
                            }
                            else
                            {
                                MessageBox.Show($"解析参数失败:{parmstr}存在不可识别字符");
                                return;
                            }
                        }
                        else
                        {
                            parmstr = inputparmstr;
                        }
                        var parms = parmstr.Split(" ");
                        var typeinfo = TypeInfos.FirstOrDefault(o => o.Name == parms[0]);
                        if (typeinfo == null)
                        {
                            MessageBox.Show($"解析参数失败:{parmstr}存在不可解析类型{parms[0]}");
                            return;
                        }
                        sourceList.Add(new DevicePCanParmDataGrid()
                        {
                            ID = sourceList.Count + 1,
                            Index = sourceList.Count,
                            Name = parms[1][0..(parms[1].Length-1)],
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
        [GeneratedRegex(@"[^a-zA-Z0-9; _]")]
        private static partial Regex ParmRegex();
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
            new TypeInfo(){Name="double",TargetType=typeof(double),FullName=typeof(double).FullName,Size=Marshal.SizeOf(typeof(double))},
            new TypeInfo(){Name="bool",TargetType=typeof(bool),FullName=typeof(bool).FullName,Size=Marshal.SizeOf(typeof(bool))},
            new TypeInfo(){Name="char",TargetType=typeof(char),FullName=typeof(char).FullName,Size=Marshal.SizeOf(typeof(char))},
        ];
    }
}
