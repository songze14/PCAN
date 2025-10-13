using Microsoft.Win32;
using PCAN.ViewModel.USercontrols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.IO.Hashing;
using System.Windows;
using System.Runtime.ConstrainedExecution;
using PCAN.Shard.Tools;
using DynamicData;

namespace PCAN.ViewModel.RunPage
{
    public class UploadPageViewModel:ReactiveObject
    {
        public UploadPageViewModel(PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel)
        {
            PCanClientUsercontrolViewModel = pCanClientUsercontrolViewModel;
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
                //1.拆分文件
                var filebytes = System.IO.File.ReadAllBytes(SelectedFilePath);
                if (filebytes==null)
                {
                    MessageBox.Show("空文件");
                    return;
                }
                //先按照512分组
                var packet512s = new List<byte[]>();
                for (int i = 0; i < filebytes.Length; i += 512)
                {
                    var chunk = filebytes.Skip(i).Take(512).ToArray();
                    packet512s.Add(chunk);
                }
                //h获取CRC
                var crcHash= CRC.CalculateCRC8(filebytes);
                //2.发送升级命令
                for (int i = 0; i < packet512s.Count; i++)
                {
                    var packet= packet512s[i];
                
                    var packet8s = new List<byte[]>();
                    //3.继续拆分成8字节的包
                    for (int j = 0; j < packet.Length; j += 8)
                    {
                        var chunk = packet.Skip(j).Take(8).ToArray();
                        packet8s.Add(chunk);
                    }
                    //4.发送开始帧
                    var startFrame=new byte[8];
                    //4.1 总长度
                    var totalLengthBytes = BitConverter.GetBytes((ushort)packet.Length);
                    totalLengthBytes.CopyTo(startFrame, 0);
                    //4.2 总包数
                    var totalPacketCountBytes = BitConverter.GetBytes((ushort)packet512s.Count);
                    totalPacketCountBytes.CopyTo(startFrame, 2);
                    //4.3 当前包序号
                    var packetindex= BitConverter.GetBytes((ushort)(i+1));
                    packetindex.CopyTo(startFrame, 4);
                    startFrame[6]=crcHash;
                    //PCanClientUsercontrolViewModel.WriteMsg(0x731, startFrame);

                    foreach (var packet8 in packet8s)
                    {
                        //4.发送8字节的包
                        //PCanClientUsercontrolViewModel.WriteMsg(0x732, packet8);
                        System.Threading.Thread.Sleep(PCanClientUsercontrolViewModel.FrameInterval);
                    }
                    var crcpacket= CRC.CalculateCRC8(packet);
                    var endFrame = new byte[8];
                    endFrame[0] = crcpacket;
                    //PCanClientUsercontrolViewModel.WriteMsg(0x733, endFrame);

                }

            });
        }
        [Reactive]
        public string SelectedFilePath { get; set; }
        public ReactiveCommand<Unit,Unit> BrowseFileCommand { get; set; }
        public ReactiveCommand<Unit, Unit> UploadCommand { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
    }
}
