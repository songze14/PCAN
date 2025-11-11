using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.SqlLite.Model
{
    [Table("DataMonitoringSetting")]
    public class DataMonitoringSetting
    {

    }
    [Table("DataMonitoringSettingDataParm")]
    public class DataMonitoringSettingDataParm 
    {
        public int Index { get; set; }
        public string Type { get; set; }
        
        public string Name { get; set; }
    }
}
