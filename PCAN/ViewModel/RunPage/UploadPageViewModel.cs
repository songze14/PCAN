using DynamicData;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PCAN.Modles;
using PCAN.Notification.Log;
using PCAN.Shard.Tools;
using PCAN.Tools;
using PCAN.ViewModel.USercontrols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Aes = System.Security.Cryptography.Aes;
using Unit = System.Reactive.Unit;


namespace PCAN.ViewModel.RunPage
{
    public class UploadPageViewModel:ReactiveObject
    {
        private readonly IMediator _mediator;

        public UploadPageViewModel(PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel,IMediator mediator)
        {
            PCanClientUsercontrolViewModel = pCanClientUsercontrolViewModel;
            _mediator = mediator;
            _cancellationtokensource= new CancellationTokenSource();
            _timecancellationtokensource=new CancellationTokenSource();
            BrowseFileCommand = ReactiveCommand.Create(() =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "升级文件/bin|*.bin",
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    SelectedFilePath = openFileDialog.FileName;
                    var safepath = openFileDialog.SafeFileName;
                    MCU = safepath.Split('_')[3][0..2];
                }
            }
            );
            UploadCommand = ReactiveCommand.Create(() =>
            {
                if (IsUploading)
                {
                    MessageBox.Show("正在升级中，请勿重复点击");
                    return;
                }
                _cancellationtokensource = new CancellationTokenSource();
                if (string.IsNullOrEmpty(SelectedFilePath) )
                {
                    MessageBox.Show("升级文件未选择" );
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
                        var filebyteaess = System.IO.File.ReadAllBytes(SelectedFilePath);
                        var key = filebyteaess[0..16];
                        if (key[4] != AESKey[4])
                        {
                            MessageBox.Show("升级文件异常！");
                            IsUploading = false;
                            return;
                        }
                        var filebytes = filebyteaess[16..];
                        var aesAlg = Aes.Create();
                        var decryptor = aesAlg.CreateDecryptor(AESKey, AESIV);
                        filebytes = decryptor.TransformFinalBlock(filebytes, 0, filebytes.Length);
                        //释放解密器
                        decryptor.Dispose();
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"已读取文件：文件大小{filebytes.Length}Byte" });
                        if (filebytes == null)
                        {
                            MessageBox.Show("空文件！");
                            IsUploading = false;
                            return;
                        }
                        //先按照PackSize分组（默认512）
                        for (int i = 0; i < filebytes.Length; i += PackSize)
                        {
                            var chunk = filebytes.Skip(i).Take(PackSize).ToArray();
                            _sourceUploadDataGridModels.Add(new UploadDataGridModel()
                            {
                                Data = chunk,
                                Index = _sourceUploadDataGridModels.Count + 1,
                                Size = $"{chunk.Length}Byte",
                                CRC = CRC.CalculateCRC8(chunk)

                            });
                           
                        }
                        await Task.Delay(1000);

                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"已分包：包数量{_sourceUploadDataGridModels.Count}" });

                    
                        //h获取CRC
                        var crcHash = CRC.CalculateCRC8(filebytes);
                        //2.发送升级命令
                        var commandFrame = new byte[8];
                   
                            int driveid = Convert.ToUInt16(PCanClientUsercontrolViewModel.DeviceID,16);
                            if (driveid==0)
                            {
                                MessageBox.Show("设备ID错误");
                                IsUploading = false;
                                return;
                            }
                            var dirveridBytes = BitConverter.GetBytes((ushort)driveid);
                            dirveridBytes.CopyTo(commandFrame, 0);
                            switch (MCU)
                            {
                                case "U0":
                                    commandFrame[4] = 0x00;
                                    break;
                                case "U1":
                                    commandFrame[4] = 0x01;
                                    break;
                                default:
                                    break;
                            }
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"发送升级指令" });
                            PCanClientUsercontrolViewModel.WriteMsg(0x730, commandFrame, () =>
                            {
                                Reset();
                            });
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"等待回复！" });
                            _semaphoreslim.Wait();
                       
                            if (UploadStep != UploadStep.Next)
                            {
                                await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"回复异常退出升级" });
                                UploadStep = UploadStep.NON;
                                IsUploading = false;

                                return;
                            }
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"回复正常,升级继续" });
                   
                        int ResendCount = 0;
                        for (int i = 0; i < _sourceUploadDataGridModels.Count; i++)
                        {
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"开始发送第{i + 1}个包" });

                            var packet = _sourceUploadDataGridModels.Items[i];
                            var packet8s = new List<byte[]>();
                            //3.继续拆分成8字节的包
                            for (int j = 0; j < packet.Data.Length; j += 8)
                            {
                                var chunk = packet.Data.Skip(j).Take(8).ToArray();
                                packet8s.Add(chunk);
                            }
                            //4.发送开始帧
                            var startFrame = new byte[8];
                            //4.1 总长度
                            var totalLengthBytes = BitConverter.GetBytes((ushort)packet.Data.Length);
                            Array.Reverse(totalLengthBytes);
                            totalLengthBytes.CopyTo(startFrame, 0);
                            //4.2 总包数
                            var totalPacketCountBytes = BitConverter.GetBytes((ushort)_sourceUploadDataGridModels.Count);
                            Array.Reverse(totalPacketCountBytes);
                            totalPacketCountBytes.CopyTo(startFrame, 2);
                            //4.3 当前包序号
                            var packetindex = BitConverter.GetBytes((ushort)(i + 1));
                            Array.Reverse(packetindex);
                            packetindex.CopyTo(startFrame, 4);
                            startFrame[6] = crcHash;
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"开始发送第{i + 1}个包的开始帧" });
                            PCanClientUsercontrolViewModel.WriteMsg(0x731, startFrame);
                            await Task.Delay(PCanClientUsercontrolViewModel.FrameInterval);
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"开始发送第{i + 1}个包的数据帧" });
                            foreach (var packet8 in packet8s)
                            {
                                PCanClientUsercontrolViewModel.WriteMsg(0x732, packet8);
                                await Task.Delay(PCanClientUsercontrolViewModel.FrameInterval);
                            }
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"开始发送第{i + 1}个包的结束帧" });
                            var endFrame = new byte[8];
                            endFrame[0] = packet.CRC;
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"开始发送第{i + 1}个包的CRC{packet.CRC}" });
                            PCanClientUsercontrolViewModel.WriteMsg(0x733, endFrame, () =>
                            {
                                Reset();
                            });
                            await Task.Delay(PCanClientUsercontrolViewModel.FrameInterval);
                            await _semaphoreslim.WaitAsync();
                       
                            switch (UploadStep)
                            {
                                case UploadStep.Next:
                                    packet.IsOver= true;
                                    break;
                                case UploadStep.BackPacket:
                                    packet.SendCount++;
                                    if (packet.SendCount >= MaxResendCount)
                                    {
                                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"已重发{ResendCount},超出5退出升级！" });
                                        IsUploading = false;
                                        return;
                                    }
                                    i--;
                                    break;
                                case UploadStep.Completed:
                                case UploadStep.TimeOut:
                                case UploadStep.NON:
                                    IsUploading = false;
                                    return;
                                default:
                                    break;
                            }
                            UIHelper.RunInUIThread(_ =>
                            {
                                UploadProgress = (int)(((i + 1) / (float)_sourceUploadDataGridModels.Count) * 100);

                            });
                        }
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"升级结束" });
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
                                    UploadStep = UploadStep.Next;
                                    break;
                                case 0x01:
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"下位回复1，升级出现错误，重发" });

                                    UploadStep = UploadStep.BackPacket;
                                    break;
                                case 0x02:
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"下位回复2，升级结束后APP区CRC校验不过" });

                                    UploadStep = UploadStep.Completed;
                                    break;
                                case 0x03:
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"下位回复3，芯片型号不匹配,退出升级" });
                                    UploadStep = UploadStep.Completed;
                                    break;
                                default:
                                    break;
                            }
                            if (_semaphoreslim.CurrentCount==0)
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
            this.ReloadCommand = ReactiveCommand.Create(() =>
            {
                _timecancellationtokensource.Cancel();
                _cancellationtokensource.Cancel();
                UploadStep = UploadStep.NON;
                _semaphoreslim=new SemaphoreSlim(0,1);
                UploadProgress = 0;
                _sourceUploadDataGridModels.Clear();
                IsUploading = false;
                MessageBox.Show("信号初始化完成");
            });
            this.EncryptionFileCommand = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrEmpty(SelectedFilePath))
                {
                    MessageBox.Show("升级文件未选择");
                    return;
                }
                var filebytes = System.IO.File.ReadAllBytes(SelectedFilePath);
                if (filebytes == null)
                {
                    MessageBox.Show("空文件！");
                    return;
                }
                
                using (var aesAlg =new AesCng())
                {
                    
                    // 创建加密器执行流转换
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(AESKey, AESIV);
                    var crysteam = encryptor.TransformFinalBlock(filebytes, 0, filebytes.Length);
                    //加入标志位
                    var newbytes= new byte[crysteam.Length + 16];
                    AESKey.CopyTo(newbytes, 0);
                    crysteam.CopyTo(newbytes, 16);
                    var newfilepath = SelectedFilePath.Replace(".bin", "_en.bin");
                    // 将所有数据写入流
                    using (var fs = new FileStream(newfilepath, FileMode.Create, FileAccess.Write))
                    {
                         fs.Write(newbytes, 0, newbytes.Length);
                    }
                    MessageBox.Show($"加密完成，已生成新文件{newfilepath}，请使用新文件进行升级！");
                }



            });
            this.ChangeObs = this._sourceUploadDataGridModels.Connect();

            var d = this.ChangeObs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _uploadDataGridModels)
                .DisposeMany()
                .Subscribe();
        }
        private byte[] AESKey = System.Text.Encoding.UTF8.GetBytes("greenworksEGG123");
        private byte[] AESIV = System.Text.Encoding.UTF8.GetBytes("greenworskEGG123");
        private async Task Reset()
        {
            try
            {
                _timecancellationtokensource = new CancellationTokenSource();
                var periodictimer = new PeriodicTimer(TimeSpan.FromSeconds(TimeOutSeconds));
                while (await periodictimer.WaitForNextTickAsync(_timecancellationtokensource.Token))
                {
                    _cancellationtokensource.Cancel();
                    UploadStep = UploadStep.TimeOut;
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
        [Reactive]
        
        public bool IsUploading { get; set; }
        [Reactive]
        public string MCU { get; set; }
        [Reactive]
        public int PackSize { get; set; } = 512;

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
        public ReactiveCommand<Unit,Unit> EncryptionFileCommand { get; set; }
        public ReactiveCommand<Unit,Unit> BrowseFileCommand { get; set; }
        public ReactiveCommand<Unit, Unit> UploadCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ReloadCommand { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
        private UploadStep UploadStep;
        private SemaphoreSlim _semaphoreslim = new SemaphoreSlim(0, 1);
        private  CancellationTokenSource _cancellationtokensource;
        private CancellationTokenSource _timecancellationtokensource;
        public IObservable<IChangeSet<UploadDataGridModel>> ChangeObs { get; }

        public SourceList<UploadDataGridModel> _sourceUploadDataGridModels = new SourceList<UploadDataGridModel>();
        private readonly ReadOnlyObservableCollection<UploadDataGridModel> _uploadDataGridModels;
        public ReadOnlyObservableCollection<UploadDataGridModel> UploadDataGridModels => _uploadDataGridModels;
    }
    internal enum UploadStep
    {
        NON,
        //下个包
        Next,
        //上个包重发
        BackPacket,
        //结束
        Completed,
        TimeOut

    }
}
