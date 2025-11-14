using DynamicData;
using DynamicData.Binding;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PCAN.Modles;
using PCAN.Notification.Log;
using PCAN.Shard.Tools;
using PCAN.Tools;
using PCAN.View.Windows;
using PCAN.ViewModel.USercontrols;
using PCAN.ViewModel.Window;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat.ModeDetection;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Windows;
using Unit = System.Reactive.Unit;

namespace PCAN.ViewModel.RunPage
{
    public class DeviceParmTuningPageViewModel:ReactiveObject
    {
        private readonly IMediator _mediator;

        public DeviceParmTuningPageViewModel(IMediator mediator, PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel)
        {
            _mediator = mediator;
            PCanClientUsercontrolViewModel = pCanClientUsercontrolViewModel;
            ParmDataGridSource
               .Connect()
               .Sort(SortExpressionComparer<PCanParmDataGrid>.Ascending(x => x.ID)) // 排序
               .Sort(SortExpressionComparer<PCanParmDataGrid>.Ascending(x => x.Index)) // 排序
               .Bind(out _parmDataGridItems)
               .Subscribe();
            this.PCanClientUsercontrolViewModel.NewMessage.Subscribe(async msg =>
            {
                try
                {
                    if (msg == null)
                        return;
                    switch (msg.ID)
                    {
                        case 0x770:
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "收到回复，开始接收数据！" });
                            _parmDatas = [];
                            break;
                        case 0x752:
                            _parmDatas.Add(msg.DATA);
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "收到数据！" });
                            break;
                        case 0x772:
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "数据收取完成，正在解析！" });

                            UIHelper.RunInUIThread((d) =>
                            {
                                try
                                {
                                    var parmbyts = _parmDatas.ToArray();
                                    var allbytes = parmbyts.Where(innerArray => innerArray != null).SelectMany(innerArray => innerArray).ToArray();
                                    var datas = ParmDataGridSource.Items.OrderBy(o => o.Index).ToList();
                                    int datasub = 0;
                                    for (var i = 0; i < datas.Count; i++)
                                    {
                                        var data = allbytes[datasub..(datasub + datas[i].Size)];
                                        datasub += datas[i].Size;
                                        ParmDataGridSource.Remove(datas[i]);
                                        var b = string.Join("", BitConverter.ToString(data).Split("-"));
                                        string value = CTypeToCsharpTypeValue.GetParmValue(datas[i].TargetFullName,data);
                                        datas[i].Value = value;
                                        datas[i].SetValue = value;
                                        ParmDataGridSource.Add(datas[i]);
                                    }
                                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "解析完成！" });
                                }
                                catch (Exception ex)
                                {

                                     _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"解析参数异常{ex.Message}" });

                                }


                            });
                            //解析数据
                           
                            break;
                        case 0x70A:
                            var data = msg.DATA;
                            var functionalcode = data[4];
                            if (functionalcode==1)
                            {
                                var haserr = data[5] == 0;
                                await _mediator.Publish(new LogNotification() { LogLevel = haserr? LogLevel.Information:LogLevel.Error, LogSource = LogSource.DeviceParm, Message = haserr ?"参数调整完成":"参数调试失败，报告错误！" });

                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {

                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"处理回复异常{ex.Message}" });

                }

            });
            this.ParmAddCommand = ReactiveCommand.Create(() =>
            {
                var windowviewmodle = new DeviceParmValueSettingWindowViewModel(TypeInfos, ParmDataGridSource, null);
                var window = new DeviceParmValueSettingWindow(windowviewmodle);
                window.ShowDialog();
            });
            this.ParmDeleteCommand = ReactiveCommand.Create(() =>
            {
                if (SelectData != null)
                {
                    ParmDataGridSource.Remove(SelectData);
                }
            });
            this.ParmEditCommand = ReactiveCommand.Create(() =>
            {
                if (SelectData != null)
                {
                   
                    var windowviewmodle = new DeviceParmValueSettingWindowViewModel(TypeInfos, ParmDataGridSource, SelectData);
                    var window = new DeviceParmValueSettingWindow(windowviewmodle);
                    window.ShowDialog();
                }

            });
            this.SaveParmFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                try
                {
                    var openFileDialog = new SaveFileDialog
                    {
                        Filter = "参数文件|*.Json",
                    };
                    if (openFileDialog.ShowDialog() != true)
                    {
                        return;
                    }
                    var saveselectedFilePath = openFileDialog.FileName;
                    if (string.IsNullOrEmpty(saveselectedFilePath))
                    {
                        MessageBox.Show(saveselectedFilePath + "文件路径不能为空", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    foreach (var item in ParmDataGridSource.Items)
                    {
                        item.Value = "";
                    }
                    var parmstr = JsonSerializer.Serialize(ParmDataGridSource.Items, new JsonSerializerOptions()
                    {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    });
                    File.WriteAllText(saveselectedFilePath, parmstr);
                    MessageBox.Show("保存完成");

                }
                catch (Exception ex)
                {
                   await _mediator.Publish(new LogNotification() { Message = $"保存文件错误:{ex.Message}", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error, LogSource = LogSource.DeviceParm });

                }

            });
            this.BrowseFileCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    var openFileDialog = new OpenFileDialog
                    {
                        Filter = "参数文件|*.Json",
                    };
                    if (openFileDialog.ShowDialog() == true)
                    {
                        SelectedFilePath = openFileDialog.FileName;

                    }
                }
                catch (Exception ex)
                {

                    _mediator.Publish(new LogNotification() { Message = $"选取文件错误:{ex.Message}", LogLevel = Microsoft.Extensions.Logging.LogLevel.Error, LogSource = LogSource.DeviceParm });

                }

            });
            this.LoadParmFileCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(SelectedFilePath))
                    {
                        MessageBox.Show(SelectedFilePath + "文件路径不能为空", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var parmstr = File.ReadAllText(SelectedFilePath);
                    ParmDataGridSource.Clear();
                    var parmDataGridSource = JsonSerializer.Deserialize<List<DevicePCanParmDataGrid>>(parmstr);
                    parmDataGridSource?.ForEach(p => ParmDataGridSource.Add(p));
                    //ParmDataGridSource.Items.OrderBy(o => o.Index);
                }
                catch (Exception ex)
                {

                    _mediator.Publish(new LogNotification() { Message = $"加载参数错误:{ex.Message}",LogLevel=Microsoft.Extensions.Logging.LogLevel.Error,LogSource=LogSource.DeviceParm });
                }
                
            });
            this.ReadParmCommand = ReactiveCommand.Create(() =>
            {
                var commandFrame = new byte[8];
                int driveid = Convert.ToUInt16(PCanClientUsercontrolViewModel.DeviceID, 16);
                if (driveid == 0)
                {
                    MessageBox.Show("设备ID错误");
                    return;
                }
                var dirveridBytes = BitConverter.GetBytes((ushort)driveid);
                dirveridBytes.CopyTo(commandFrame, 0);
                PCanClientUsercontrolViewModel.WriteMsg(0x710, commandFrame);
                 _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "已发送指令！" });


            });
            this.OpenImportParmWindowCommand = ReactiveCommand.Create(() =>
            {
                var window = new DeviceParmValueImportWindow();
                var viewmodel = new DeviceParmValueImportWindowViewModel(ParmDataGridSource);
                window.ViewModel = viewmodel;
                window.ShowDialog();
            });
            this.SendParmCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    if (_parmDatas.Count == 0)
                        return;
                    int driveid = Convert.ToUInt16(PCanClientUsercontrolViewModel.DeviceID, 16);
                    if (driveid == 0)
                    {
                        MessageBox.Show("设备ID错误");
                        return;
                    }
                    var senddatalist = new List<byte[]>();
                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "开始数据整合！" });
                    
                    foreach (var item in ParmDataGridSource.Items)
                    {
                        byte[] bytes = new byte[item.Size];
                        switch (item.TargetFullName)
                        {
                            case "char":
                                var chardatabyte=Encoding.ASCII.GetBytes(item.SetValue);
                                chardatabyte.CopyTo(bytes, 0);
                                break;
                            case "float":
                                var floatdatabyte = BitConverter.GetBytes(float.Parse(item.SetValue));
                                floatdatabyte[0..item.Size].CopyTo(bytes, 0);

                                break;
                            default:
                                var databyte = BitConverter.GetBytes(Convert.ToInt64( item.SetValue));
                                databyte[0..item.Size].CopyTo(bytes, 0);
                               
                                break;
                        }
                       
                        senddatalist.Add(bytes);
                    }
                    var parmbyts = senddatalist.SelectMany(o => o).ToArray();
                    senddatalist = [];
                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "开始数据切片分包！" });

                    //切片分包
                    for (int i = 0; i < parmbyts.Length; i += 8)
                    {
                        var chunk = parmbyts.Skip(i).Take(8).ToArray();
                        senddatalist.Add(chunk);
                    }
                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = $"数据分包{senddatalist.Count}！" });

                    var commandFrame = new byte[8];
                    var dirveridBytes = BitConverter.GetBytes((ushort)driveid);
                    dirveridBytes.CopyTo(commandFrame, 0);
                    var datalenght = BitConverter.GetBytes(parmbyts.Length);
                    datalenght.CopyTo(commandFrame, 4);
                    PCanClientUsercontrolViewModel.WriteMsg(0x713, commandFrame);
                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "开始发送参数！" });
                    var crc = CRC.CalculateCRC8(parmbyts);
                    foreach (var item in senddatalist)
                    {
                        PCanClientUsercontrolViewModel.WriteMsg(0x714, item);
                    }
                    PCanClientUsercontrolViewModel.WriteMsg(0x715, [crc]);
                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "参数发送完成！" });


                }
                catch (Exception ex)
                {

                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.DeviceParm, Message = $"发送参数时出现错误:{ex.Message}！" });

                }



            });
        }
        public ReactiveCommand<Unit, Unit> BrowseFileCommand { get; set; }

        public ReactiveCommand<Unit, Unit> ParmAddCommand { get; }
        public ReactiveCommand<Unit, Unit> ParmDeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> ParmEditCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveParmFileCommand { get; }
        public ReactiveCommand<Unit,Unit> LoadParmFileCommand { get; set; }
        public ReactiveCommand<Unit,Unit> ReadParmCommand { get; set; }
        public ReactiveCommand<Unit,Unit> SendParmCommand { get; set; }
        public ReactiveCommand<Unit,Unit> OpenImportParmWindowCommand { get; set; }
        [Reactive]
        public string SelectedFilePath { get; set; }
        [Reactive]
        public DevicePCanParmDataGrid SelectData { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
        private List<byte[]> _parmDatas =[];
        public SourceList<DevicePCanParmDataGrid> ParmDataGridSource { get; set; } = new SourceList<DevicePCanParmDataGrid>();
        private readonly ReadOnlyObservableCollection<DevicePCanParmDataGrid> _parmDataGridItems;
        public ReadOnlyObservableCollection<DevicePCanParmDataGrid> ParmDataGridCollection => _parmDataGridItems;
        public  ObservableCollection<TypeInfo> TypeInfos { get; set; }=
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
