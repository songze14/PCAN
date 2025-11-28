using PCAN.SqlLite.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.SqlLite.Abs
{
    public interface IUploadSettingService
    {
        Task<UploadSetting> GetUploadSetting();
        Task<UploadSetting> UpdateUploadSetting(UploadSetting setting);
    }
}
