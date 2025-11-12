using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.SqlLite.Model
{
    public class SQLDbContext:DbContext
    {
        public SQLDbContext(DbContextOptions<SQLDbContext> options):base(options)
        {
                
        }
        public DbSet<DataMonitoringSetting> DataMonitoringSettings { get; set; }
        public DbSet<DataMonitoringSettingDataParm> DataMonitoringSettingDataParms { get; set; }
    }
}
