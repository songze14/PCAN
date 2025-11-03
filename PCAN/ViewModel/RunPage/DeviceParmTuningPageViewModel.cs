using DynamicData;
using DynamicData.Binding;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PCAN.Modles;
using PCAN.Notification.Log;
using PCAN.Tools;
using PCAN.View.Windows;
using PCAN.ViewModel.USercontrols;
using PCAN.ViewModel.Window;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.IO;
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
                        case 0x751:
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
                                var parmbyts = _parmDatas.ToArray();
                                var allbytes = parmbyts.Where(innerArray => innerArray != null).SelectMany(innerArray => innerArray).ToArray();
                                var datas = ParmDataGridSource.Items.OrderBy(o => o.Index).ToList();
                                int datasub = 0;
                                for (var i = 0; i < datas.Count; i++)
                                {
                                    var data = allbytes[datasub..(datasub + datas[i].Size)];
                                    Array.Reverse(data);
                                    datasub += datas[i].Size;
                                    ParmDataGridSource.Remove(datas[i]);
                                    var b = string.Join("", BitConverter.ToString(data).Split("-"));
                                    datas[i].Value =Convert.ToInt64("0x"+string.Join("",BitConverter.ToString(data).Split("-")),16).ToString("X");
                                    ParmDataGridSource.Add(datas[i]);
                                }
                                _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DeviceParm, Message = "正在解析完成！" });

                            });
                            //解析数据
                           
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
            BrowseFileCommand = ReactiveCommand.Create(() =>
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

            });
        }
        public ReactiveCommand<Unit, Unit> BrowseFileCommand { get; set; }

        public ReactiveCommand<Unit, Unit> ParmAddCommand { get; }
        public ReactiveCommand<Unit, Unit> ParmDeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> ParmEditCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveParmFileCommand { get; }
        public ReactiveCommand<Unit,Unit> LoadParmFileCommand { get; set; }
        public ReactiveCommand<Unit,Unit> ReadParmCommand { get; set; }
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
