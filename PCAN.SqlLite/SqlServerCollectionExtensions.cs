using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PCAN.SqlLite.Abs;
using PCAN.SqlLite.Imp;
using PCAN.SqlLite.Model;

namespace PCAN.SqlLite
{
    public static class SqlServerCollectionExtensions
    {
        public static IServiceCollection AddSqlLite(this IServiceCollection services,IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 提取目录路径并确保目录存在
            var dbPath = connectionString.Replace("Data Source=", "").Trim();
            var directory = Path.GetDirectoryName(dbPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            services.AddDbContext<SQLDbContext>(options =>
                options.UseSqlite(connectionString));
            services.AddScoped<IDataMonitoringSettingService, DataMonitoringSettingService>();
            return services;
        }
    }
}
