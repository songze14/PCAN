using PCAN.UserControls.UploadSetting;
using PCAN.ViewModel.Usercontrols.DataMonitoringSettings;
using PCAN.ViewModel.Usercontrols.UploadSetting;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.ViewModel.RunPage
{
    public class SysTemSettingsPageViewModel : ReactiveObject
    {
        public SysTemSettingsPageViewModel(DataMonitoringPlotParmUserControlViewModel dataMonitoringSettingUserControlViewModel, UploadSettingUserControlViewModel  uploadSettingUserControlViewModel)
        {
            DataMonitoringSettingUserControlViewModel = dataMonitoringSettingUserControlViewModel;
            UploadSettingUserControlViewModel = uploadSettingUserControlViewModel;
        }

        public DataMonitoringPlotParmUserControlViewModel DataMonitoringSettingUserControlViewModel { get; }
        public UploadSettingUserControlViewModel UploadSettingUserControlViewModel { get; }
    }
}
