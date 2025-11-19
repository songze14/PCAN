using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.SqlLite.Model
{
    [Table("DataMonitoringSetting")]
    public class DataMonitoringSetting
    {
        [Key]
        public int Id { get; set; }
        public string? GetDataID { get;  set; }
        public string? StartId { get;  set; }
        public string? ReciveDataId { get;  set; }
        public string? StopId { get;  set; }

    }
    [Table("DataMonitoringSettingDataParm")]
    public class DataMonitoringSettingDataParm 
    {
        [Key]
        public int Id { get; set; }
        public int Index { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public string? Remark { get; set; }

    }
}
