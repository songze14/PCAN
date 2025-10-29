using DynamicData;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PCAN.Notification.Log;
using PCAN.Shard.Tools;
using PCAN_AutoCar_Test_Client.Modles;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unit = System.Reactive.Unit;

namespace PCAN_AutoCar_Test_Client.ViewModel
{
    public class UploadPageViewModel:ReactiveObject
    {
        private readonly IMediator _mediator;
        private int ReaciveID;
        public UploadPageViewModel(PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel, IMediator mediator)
        {
            PCanClientUsercontrolViewModel = pCanClientUsercontrolViewModel;
            _mediator = mediator;
            _cancellationtokensource = new CancellationTokenSource();
            _timecancellationtokensource = new CancellationTokenSource();
            BrowseFileCommand = ReactiveCommand.Create(async () =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "升级文件/bin|*.bin",
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    SelectedFilePath = openFileDialog.FileName;
                }
                var filebyteaess = System.IO.File.ReadAllBytes(SelectedFilePath);
                await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"已读取文件：文件大小{filebyteaess.Length}Byte" });
                if (filebyteaess == null)
                {
                    MessageBox.Show("空文件！");
                    IsUploading = false;
                    return;
                }
                //先按照PackSize分组（默认512）
                for (int i = 0; i < filebyteaess.Length; i += 64)
                {
                    var chunk = filebyteaess.Skip(i).Take(64).ToArray();
                    _sourceUploadDataGridModels.Add(new UploadDataGridModel()
                    {
                        Data = chunk,
                        Index = _sourceUploadDataGridModels.Count + 1,
                        Size = $"{chunk.Length}Byte",
                       

                    });

                }
            }
            );
            UploadCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (IsUploading)
                {
                    MessageBox.Show("正在升级中，请勿重复点击");
                    return;
                }
                var reloadresult = await Reload();
                if (!reloadresult)
                {
                    return;
                }
                _cancellationtokensource = new CancellationTokenSource();
                if (string.IsNullOrEmpty(SelectedFilePath))
                {
                    MessageBox.Show("升级文件未选择");
                    return;
                }
                if (!PCanClientUsercontrolViewModel.IsConnected)
                {
                    MessageBox.Show("请先连接设备");
                    return;
                }
                _ = Task.Run(async () =>
                {
                    try
                    {
                        IsUploading = true;
                        if (_cancellationtokensource.Token.IsCancellationRequested)
                        {
                            MessageBox.Show("线程异常，重启软件重试！");
                            IsUploading = false;

                            return;
                        }
                        //1.拆分文件
                       
                        await Task.Delay(1000);

                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"已分包：包数量{_sourceUploadDataGridModels.Count}" });

                        //2.发送升级命令
                        var commandFrame = new byte[8];

                        if (SelectedUploadDeviceValue == 0)
                        {
                            MessageBox.Show("设备ID错误");
                            IsUploading = false;
                            return;
                        }
                        //拼装升级命令
                        var sendid =(uint)(SelectedUploadDeviceValue*33554432+1*2097152+262144);
                        

                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"发送升级指令" });
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"等待回复！" });

                        while (_cancellationtokensource.IsCancellationRequested)
                        {
                            PCanClientUsercontrolViewModel.WriteMsg(sendid, commandFrame, true);
                        }
                       
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"回复正常,升级继续" });

                      
                        IsUploading = false;
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show($"升级出现错误:{ex.Message}");
                        IsUploading = false;
                        return;
                    }
                }, _cancellationtokensource.Token
                );
            });
            this.PCanClientUsercontrolViewModel.NewMessage.Subscribe(async msg =>
            {
                try
                {
                    if (msg == null)
                        return;
                    switch (msg.ID)
                    {
                        case 0x70A:
                            var data = msg.DATA[5];
                            switch (data)
                            {
                                case 0x00:
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"下位回复0，升级继续" });
                                   
                                    break;
                                case 0x01:
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"下位回复1，升级出现错误，重发" });

                                   
                                    break;
                                case 0x02:
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"下位回复2，升级结束后APP区CRC校验不过" });

                                   
                                    break;
                                case 0x03:
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"下位回复3，芯片型号不匹配,退出升级" });
                                  
                                    break;
                                default:
                                    break;
                            }
                            if (_semaphoreslim.CurrentCount == 0)
                            {
                                _semaphoreslim.Release();

                            }
                            _timecancellationtokensource.Cancel();
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

            
            this.ChangeObs = this._sourceUploadDataGridModels.Connect();

            var d = this.ChangeObs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _uploadDataGridModels)
                .DisposeMany()
                .Subscribe();
        }
       
        private async Task Reset()
        {
            try
            {
                _timecancellationtokensource = new CancellationTokenSource();
                var periodictimer = new PeriodicTimer(TimeSpan.FromSeconds(TimeOutSeconds));
                while (await periodictimer.WaitForNextTickAsync(_timecancellationtokensource.Token))
                {
                    _cancellationtokensource.Cancel();
                   
                    if (_semaphoreslim.CurrentCount == 0)
                    {
                        _semaphoreslim.Release();

                    }
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"信息回复超时" });
                    IsUploading = false;
                    return;
                }
            }
            catch (Exception ex)
            {
            }
        }
        private async Task<bool> Reload()
        {
            try
            {
                _timecancellationtokensource.Cancel();
                _cancellationtokensource.Cancel();
              
                _semaphoreslim = new SemaphoreSlim(0, 1);
                UploadProgress = 0;
                _sourceUploadDataGridModels.Clear();
                IsUploading = false;
                await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"信号初始化完成" });
                return true;
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"信号初始化时出现错误:{ex.Message}" });

                return false;

            }

        }
        [Reactive]

        public bool IsUploading { get; set; }
      
      
      

        [Reactive]

        public int MaxResendCount { get; set; } = 5;

        [Reactive]
        public int TimeOutSeconds { get; set; } = 5;

        [Reactive]
        public string SelectedFilePath { get; set; }
        [Reactive]
        public int UploadProgress { get; set; }
        /// <summary>
        /// 加密文件
        /// </summary>
        public ReactiveCommand<Unit, Unit> EncryptionFileCommand { get; set; }
        public ReactiveCommand<Unit, Task> BrowseFileCommand { get; set; }
        public ReactiveCommand<Unit, Unit> UploadCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ReloadCommand { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
       
        private SemaphoreSlim _semaphoreslim = new(0, 1);
        private CancellationTokenSource _cancellationtokensource;
        private CancellationTokenSource _timecancellationtokensource;
        public IObservable<IChangeSet<UploadDataGridModel>> ChangeObs { get; }

        public SourceList<UploadDataGridModel> _sourceUploadDataGridModels = new SourceList<UploadDataGridModel>();
        private readonly ReadOnlyObservableCollection<UploadDataGridModel> _uploadDataGridModels;
        public ReadOnlyObservableCollection<UploadDataGridModel> UploadDataGridModels => _uploadDataGridModels;
        [Reactive]
        public int SelectedUploadDeviceValue { get; set; }
        public ObservableCollection<UploadDevice> UploadDevices { get; set; }=new ObservableCollection<UploadDevice>()
        {
            new UploadDevice(){Name="主控板",Value=3},
            new UploadDevice(){Name="后右舵板",Value=5},
            new UploadDevice(){Name="后左舵板",Value=6},
            new UploadDevice(){Name="电池板",Value=10},
        
        };
    }
}
