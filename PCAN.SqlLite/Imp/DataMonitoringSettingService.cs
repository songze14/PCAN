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
    public class DataMonitoringSettingService : IDataMonitoringSettingService
    {
        private readonly SQLDbContext _dbcontext;

        public DataMonitoringSettingService(SQLDbContext dbContext)
        {
            _dbcontext = dbContext;
        }
        public async Task<DataMonitoringSetting?> GetDataMonitoringSetting()
        {
            return await _dbcontext.DataMonitoringSettings.FirstOrDefaultAsync();
        }

        public async Task<List<DataMonitoringSettingDataParm>> GetDataMonitoringSettingDataParms()
        {
            return await _dbcontext.DataMonitoringSettingDataParms.ToListAsync();

        }

        public async Task<DataMonitoringSetting> RemoveDataMonitoringSetting(DataMonitoringSetting data)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DataMonitoringSettingDataParm>> RemoveDataMonitoringSetting(DataMonitoringSettingDataParm data)
        {
            throw new NotImplementedException();
        }

        public async Task<DataMonitoringSetting> UpdateDataMonitoringSetting(DataMonitoringSetting data)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DataMonitoringSettingDataParm>> AddDataMonitoringSettingDataParms(List<DataMonitoringSettingDataParm> datas)
        {
            using (var trance = _dbcontext.Database.BeginTransaction())
            {
                try
                {
                    var olddata = await _dbcontext.DataMonitoringSettingDataParms.ToListAsync();
                    if (olddata.Count != 0)
                    {
                        _dbcontext.DataMonitoringSettingDataParms.RemoveRange(olddata);
                        await _dbcontext.SaveChangesAsync();

                    }
                    datas.ForEach(o => o.Id = 0);
                    await _dbcontext.DataMonitoringSettingDataParms.AddRangeAsync(datas);
                    await _dbcontext.SaveChangesAsync();
                    await trance.CommitAsync();
                }
                catch (Exception)
                {
                    await trance.RollbackAsync();

                    throw;
                }
                
            }
        
            return datas;
        }
    }
}
