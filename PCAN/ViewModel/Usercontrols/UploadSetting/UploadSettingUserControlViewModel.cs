using MediatR;
using Microsoft.Win32;
using PCAN.Notification.Log;
using PCAN.SqlLite.Abs;
using PCAN.SqlLite.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.IO;
using System.Reactive;
using System.Security.Cryptography;
using System.Windows;
using Unit = System.Reactive.Unit;

namespace PCAN.ViewModel.Usercontrols.UploadSetting
{
    public class UploadSettingUserControlViewModel : ReactiveObject
    {
        private readonly IUploadSettingService _uploadsettingservice;
        private readonly IMediator _mediator;

        public UploadSettingUserControlViewModel(IUploadSettingService uploadSettingService, IMediator mediator)
        {
            _uploadsettingservice = uploadSettingService;
            _mediator = mediator;
            this.SaveCommand = ReactiveCommand.Create(async () =>
            {
                try
                {
                    var data = new PCAN.SqlLite.Model.UploadSetting()
                    {
                        MaxResendCount = MaxResendCount,
                        PackageSize = PackageSize,
                        TimeOutSeconds = TimeOutSeconds,
                        UploadType = IsHexUpload ? UploadType.Hex : UploadType.Bin
                    };
                    await _uploadsettingservice.UpdateUploadSetting(data);
                }
                catch (Exception ex)
                {

                   await _mediator.Publish(new LogNotification() { LogLevel=Microsoft.Extensions.Logging.LogLevel.Error,LogSource= LogSource.Upload, Message=$"更新设置出现错误{ex.Message}"});
                }
                
            });
            this.RefCommand= ReactiveCommand.Create(async () => 
            {
                try
                {
                    var data = await _uploadsettingservice.GetUploadSetting();
                    if (data!=null)
                    {
                        PackageSize = data.PackageSize;
                        TimeOutSeconds = data.TimeOutSeconds;
                        MaxResendCount = data.MaxResendCount;
                        IsHexUpload = data.UploadType == UploadType.Hex;
                    }
                  
                }
                catch (Exception ex)
                {

                    await _mediator.Publish(new LogNotification() { LogLevel = Microsoft.Extensions.Logging.LogLevel.Error, LogSource = LogSource.Upload, Message = $"获取设置出现错误{ex.Message}" });
                }
             
            } );
            this.EncryptionFileCommand = ReactiveCommand.Create(() =>
            {
                var filesuffix = IsHexUpload ? "hex" : "bin";
                var openFileDialog = new OpenFileDialog
                {
                    Filter =$"升级文件/{filesuffix}|*.{filesuffix}" ,
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    var selectedFilePath = openFileDialog.FileName;
               
                    if (string.IsNullOrEmpty(selectedFilePath))
                    {
                        MessageBox.Show("升级文件未选择");
                        return;
                    }
                    var filebytes = System.IO.File.ReadAllBytes(selectedFilePath);
                    if (filebytes == null)
                    {
                        MessageBox.Show("空文件！");
                        return;
                    }
                    using (var aesAlg = new AesCng())
                    {

                        // 创建加密器执行流转换
                        ICryptoTransform encryptor = aesAlg.CreateEncryptor(AESKey, AESIV);
                        var crysteam = encryptor.TransformFinalBlock(filebytes, 0, filebytes.Length);
                        //加入标志位
                        var newbytes = new byte[crysteam.Length + 16];
                        AESKey.CopyTo(newbytes, 0);
                        crysteam.CopyTo(newbytes, 16);
                        var newfilepath = selectedFilePath.Replace($".{filesuffix}", $"_en.{filesuffix}");
                        // 将所有数据写入流
                        using (var fs = new FileStream(newfilepath, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(newbytes, 0, newbytes.Length);
                        }
                        MessageBox.Show($"加密完成，已生成新文件{newfilepath}，请使用新文件进行升级！");
                    }
                }
              



            });
            RefCommand.Subscribe();
        }
        private byte[] AESKey = System.Text.Encoding.UTF8.GetBytes("greenworksEGG123");
        private byte[] AESIV = System.Text.Encoding.UTF8.GetBytes("greenworskEGG123");
        /// <summary>
        /// 加密文件
        /// </summary>
        public ReactiveCommand<Unit, Unit> EncryptionFileCommand { get; set; }
        public ReactiveCommand<Unit, Task> SaveCommand { get; set; }
        public ReactiveCommand<Unit, Task> RefCommand { get; set; }
        [Reactive]
        public int PackageSize { get; set; } = 512;

        [Reactive]
        public int MaxResendCount { get; set; } = 5;

        [Reactive]
        public int TimeOutSeconds { get; set; } = 5;
        [Reactive]
        public bool IsHexUpload { get; set; }
    }
}
