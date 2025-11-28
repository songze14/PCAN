using Microsoft.EntityFrameworkCore;
using PCAN.SqlLite.Abs;
using PCAN.SqlLite.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.SqlLite.Imp
{
    public class UploadSettingService : IUploadSettingService
    {
        private readonly SQLDbContext _dbcontext;

        public UploadSettingService(SQLDbContext dbContext)
        {
            _dbcontext = dbContext;
        }
        public async Task<UploadSetting?> GetUploadSetting()
        {
            return await _dbcontext.UploadSettings.FirstOrDefaultAsync();
        }

        public async Task<UploadSetting> UpdateUploadSetting(UploadSetting setting)
        {
            var olddata =await _dbcontext.UploadSettings.FirstOrDefaultAsync();
            if (olddata != null) 
            {
                olddata.TimeOutSeconds= setting.TimeOutSeconds;
                olddata.MaxResendCount= setting.MaxResendCount;
                olddata.PackageSize= setting.PackageSize;
                olddata.UploadType= setting.UploadType;
                _dbcontext.Update(olddata);
            }
            else
            {
                await _dbcontext.AddAsync(setting);
            }
            await _dbcontext.SaveChangesAsync();
            return setting;
        }
    }
}
