using MediatR;
using PCAN.Notification.Log;
using PCAN.SqlLite.Abs;
using PCAN.SqlLite.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
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
                    PackageSize = data.PackageSize;
                    TimeOutSeconds = data.TimeOutSeconds;
                    MaxResendCount = data.MaxResendCount;
                    IsHexUpload = data.UploadType == UploadType.Hex;
                }
                catch (Exception ex)
                {

                    await _mediator.Publish(new LogNotification() { LogLevel = Microsoft.Extensions.Logging.LogLevel.Error, LogSource = LogSource.Upload, Message = $"获取设置出现错误{ex.Message}" });
                }
             
            } );
            RefCommand.Subscribe();
        }
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
