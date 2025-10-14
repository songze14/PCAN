using DynamicData;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PCAN.Notification.Log;
using PCAN.Shard.Tools;
using PCAN.ViewModel.USercontrols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.IO.Hashing;
using System.Reactive;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
                // Implementation for browsing a file
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "All Files|*.*|Text Files|*.txt|CSV Files|*.csv",
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    SelectedFilePath = openFileDialog.FileName;
                }
            }
                
            );
            UploadCommand = ReactiveCommand.Create(() =>
            {
                //_cancellationtokensource.Cancel();
                _cancellationtokensource = new CancellationTokenSource();
                //_timecancellationtokensource = new CancellationTokenSource();
                // Implementation for uploading the file
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
                    if (_cancellationtokensource.Token.IsCancellationRequested)
                    {
                        MessageBox.Show("线程异常");
                        return;
                    }
                    //1.拆分文件
                    var filebytes = System.IO.File.ReadAllBytes(SelectedFilePath);
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"已读取文件：文件大小{filebytes.Length}Byte" });
                    if (filebytes == null)
                    {
                        MessageBox.Show("空文件");
                        return;
                    }
                    //判断文件大小是否可以%8无余数
                    //var length8 = filebytes.Length % 8;
                    //if (length8 != 0)
                    //{
                    //    Array.Resize(ref filebytes, filebytes.Length +  length8);
                    //}
                    //先按照512分组
                    var packet512s = new List<byte[]>();
                    for (int i = 0; i < filebytes.Length; i += 512)
                    {
                        var chunk = filebytes.Skip(i).Take(512).ToArray();
                        packet512s.Add(chunk);
                    }
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"已分包：包数量{packet512s.Count}" });
                    
                    //h获取CRC
                    var crcHash = CRC.CalculateCRC8(filebytes);
                    //2.发送升级命令
                    var commandFrame = new byte[8];
                    try
                    {
                        int driveid = Convert.ToUInt16(PCanClientUsercontrolViewModel.DeviceID,16);
                        if (driveid==0)
                        {
                            MessageBox.Show("设备ID错误");
                            return;
                        }
                        var dirveridBytes = BitConverter.GetBytes((ushort)driveid);
                        dirveridBytes.CopyTo(commandFrame, 0);
                        commandFrame[4] = 0x01;
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

                            return;
                        }
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"回复正常,升级继续" });
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show($"升级出现错误:{ex.Message}");
                        UploadStep = UploadStep.NON;
                        return;
                    }
                    int ResendCount = 0;
                    for (int i = 0; i < packet512s.Count; i++)
                    {
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"开始发送第{i + 1}个包" });

                        var packet = packet512s[i];
                        var packet8s = new List<byte[]>();
                        //3.继续拆分成8字节的包
                        for (int j = 0; j < packet.Length; j += 8)
                        {
                            var chunk = packet.Skip(j).Take(8).ToArray();
                            packet8s.Add(chunk);
                        }
                        //4.发送开始帧
                        var startFrame = new byte[8];
                        //4.1 总长度
                        var totalLengthBytes = BitConverter.GetBytes((ushort)packet.Length);
                        Array.Reverse(totalLengthBytes);
                        totalLengthBytes.CopyTo(startFrame, 0);
                        //4.2 总包数
                        var totalPacketCountBytes = BitConverter.GetBytes((ushort)packet512s.Count);
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
                            //4.发送8字节的包
                            //if (packet8.Count()<8)
                            //{
                            //    var data = packet8;
                            //    packet8 =new byte[8];
                            //}
                            PCanClientUsercontrolViewModel.WriteMsg(0x732, packet8);
                            await Task.Delay(PCanClientUsercontrolViewModel.FrameInterval);
                        }
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"开始发送第{i + 1}个包的结束帧" });

                        var crcpacket = CRC.CalculateCRC8(packet);
                        var endFrame = new byte[8];
                        endFrame[0] = crcpacket;
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"开始发送第{i + 1}个包的CRC{Convert.ToString(crcpacket,16)}" });
                        PCanClientUsercontrolViewModel.WriteMsg(0x733, endFrame, () =>
                        {
                           Reset();
                        });
                        await Task.Delay(PCanClientUsercontrolViewModel.FrameInterval);

                        await _semaphoreslim.WaitAsync();
                        switch (UploadStep)
                        {
                            case UploadStep.Next:
                                break;
                            case UploadStep.BackPacket:
                                ResendCount++;
                                if (ResendCount>=5)
                                {
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"已重发{ResendCount},超出5退出升级！" });
                                    return;
                                }
                                i--;
                                break;
                            case UploadStep.Completed:
                            case UploadStep.TimeOut:
                            case UploadStep.NON:
                                return;
                            default:
                                break;
                        }
                    }
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"升级结束" });

                },_cancellationtokensource.Token
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
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"下位回复1，升级出现错误，重发" });

                                    UploadStep = UploadStep.BackPacket;
                                    break;
                                case 0x02:
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"下位回复2，升级结束后APP区CRC校验不过" });

                                    UploadStep = UploadStep.Completed;
                                    break;
                                case 0x03:
                                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"下位回复3，芯片型号不匹配,退出升级" });
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

                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"处理回复异常{ex.Message}" });

                }

            });
            this.ReloadCommand = ReactiveCommand.Create(() =>
            {
                _timecancellationtokensource.Cancel();
                _cancellationtokensource.Cancel();
                UploadStep = UploadStep.NON;
                _semaphoreslim=new SemaphoreSlim(0,1);
                //if (_semaphoreslim.CurrentCount == 0)
                //{
                //    _semaphoreslim.Release();

                //}
                MessageBox.Show("信号初始化完成");
            });
        }
        private async Task Reset()
        {
            try
            {
                _timecancellationtokensource = new CancellationTokenSource();
                var periodictimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
                while (await periodictimer.WaitForNextTickAsync(_timecancellationtokensource.Token))
                {
                    _cancellationtokensource.Cancel();
                    UploadStep = UploadStep.TimeOut;
                    if (_semaphoreslim.CurrentCount == 0)
                    {
                        _semaphoreslim.Release();

                    }
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.Upload, Message = $"信息回复超时" });
                    return;
                }
               
                //if (timedisponse.Result)
                //{
                //   
                //}
            }
            catch (Exception ex)
            {
            }




        }

        private PeriodicTimer _periodictimer;
        [Reactive]
        public string SelectedFilePath { get; set; }
        public ReactiveCommand<Unit,Unit> BrowseFileCommand { get; set; }
        public ReactiveCommand<Unit, Unit> UploadCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ReloadCommand { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
        private UploadStep UploadStep;
        private SemaphoreSlim _semaphoreslim = new SemaphoreSlim(0, 1);
        private  CancellationTokenSource _cancellationtokensource;
        private CancellationTokenSource _timecancellationtokensource;

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
