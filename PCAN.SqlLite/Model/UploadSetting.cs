using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.SqlLite.Model
{
    [Table("UploadSetting")]
    public class UploadSetting
    {
        [Key]
        public int Id { get; set; }
        public int MaxResendCount { get; set; }
        public int TimeOutSeconds { get; set; }
        public int PackageSize { get; set; }
        public UploadType UploadType { get; set; }
    }
    public enum UploadType
    {
        Hex,
        Bin
    }
}
