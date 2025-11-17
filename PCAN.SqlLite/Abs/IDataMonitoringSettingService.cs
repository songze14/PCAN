using PCAN.SqlLite.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.SqlLite.Abs
{
    public interface IDataMonitoringSettingService
    {
        Task<DataMonitoringSetting?> GetDataMonitoringSetting();
        Task<List< DataMonitoringSettingDataParm>> GetDataMonitoringSettingDataParms();
        Task<DataMonitoringSetting> UpdateDataMonitoringSetting(DataMonitoringSetting data);
        Task<List<DataMonitoringSettingDataParm>> AddDataMonitoringSettingDataParms(List<DataMonitoringSettingDataParm> datas);

        Task<DataMonitoringSetting> RemoveDataMonitoringSetting(DataMonitoringSetting data);
     

    }
}
