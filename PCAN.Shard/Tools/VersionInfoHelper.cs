using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Shard.Tools
{
    /// <summary>
    /// 应用程序版本信息帮助类
    /// </summary>
    public static class VersionInfoHelper
    {
        /// <summary>
        /// 获取完整的应用程序版本信息
        /// </summary>
        public static ApplicationVersionInfo GetVersionInfo()
        {
            var assembly = Assembly.GetEntryAssembly();

            return new ApplicationVersionInfo
            {
                // 版本信息
                AssemblyVersion = GetAssemblyVersion(assembly),
                FileVersion = GetFileVersion(assembly),
                InformationalVersion = GetInformationalVersion(assembly),
                ProductVersion = GetProductVersion(assembly),

                // 程序集信息
                Company = GetCompany(assembly),
                Product = GetProduct(assembly),
                Copyright = GetCopyright(assembly),
                Title = GetTitle(assembly),
                Description = GetDescription(assembly),

                // 文件信息
                FilePath = assembly?.Location,
                FileCreationTime = GetFileCreationTime(assembly),
                FileLastWriteTime = GetFileLastWriteTime(assembly)
            };
        }

        /// <summary>
        /// 获取程序集版本
        /// </summary>
        public static Version GetAssemblyVersion(Assembly assembly = null)
        {
            assembly ??= Assembly.GetEntryAssembly();
            return assembly?.GetName().Version;
        }

        /// <summary>
        /// 获取文件版本
        /// </summary>
        public static string GetFileVersion(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                var fileVersionAttr = assembly?.GetCustomAttribute<AssemblyFileVersionAttribute>();
                return fileVersionAttr?.Version;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取信息版本
        /// </summary>
        public static string GetInformationalVersion(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                var informationalVersionAttr = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                return informationalVersionAttr?.InformationalVersion;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取产品版本
        /// </summary>
        public static string GetProductVersion(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly?.Location ?? "");
                return fileVersionInfo.ProductVersion;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取公司信息
        /// </summary>
        public static string GetCompany(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                var companyAttr = assembly?.GetCustomAttribute<AssemblyCompanyAttribute>();
                return companyAttr?.Company;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取产品名称
        /// </summary>
        public static string GetProduct(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                var productAttr = assembly?.GetCustomAttribute<AssemblyProductAttribute>();
                return productAttr?.Product;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取版权信息
        /// </summary>
        public static string GetCopyright(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                var copyrightAttr = assembly?.GetCustomAttribute<AssemblyCopyrightAttribute>();
                return copyrightAttr?.Copyright;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取程序集标题
        /// </summary>
        public static string GetTitle(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                var titleAttr = assembly?.GetCustomAttribute<AssemblyTitleAttribute>();
                return titleAttr?.Title;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取程序集描述
        /// </summary>
        public static string GetDescription(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                var descriptionAttr = assembly?.GetCustomAttribute<AssemblyDescriptionAttribute>();
                return descriptionAttr?.Description;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取文件创建时间
        /// </summary>
        public static DateTime? GetFileCreationTime(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                if (assembly?.Location != null && File.Exists(assembly.Location))
                {
                    return File.GetCreationTime(assembly.Location);
                }
            }
            catch
            {
                // 忽略错误
            }
            return null;
        }

        /// <summary>
        /// 获取文件最后修改时间
        /// </summary>
        public static DateTime? GetFileLastWriteTime(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetEntryAssembly();
                if (assembly?.Location != null && File.Exists(assembly.Location))
                {
                    return File.GetLastWriteTime(assembly.Location);
                }
            }
            catch
            {
                // 忽略错误
            }
            return null;
        }

        /// <summary>
        /// 获取显示用的版本字符串
        /// </summary>
        public static string GetDisplayVersion()
        {
            var versionInfo = GetVersionInfo();
            return versionInfo.InformationalVersion ??
                   versionInfo.FileVersion ??
                   versionInfo.AssemblyVersion?.ToString() ??
                   "Unknown Version";
        }

        /// <summary>
        /// 获取简化的版本信息
        /// </summary>
        public static string GetShortVersion()
        {
            var version = GetAssemblyVersion();
            return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "Unknown";
        }
    }

    /// <summary>
    /// 应用程序版本信息类
    /// </summary>
    public class ApplicationVersionInfo
    {
        public Version AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public string InformationalVersion { get; set; }
        public string ProductVersion { get; set; }
        public string Company { get; set; }
        public string Product { get; set; }
        public string Copyright { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public DateTime? FileCreationTime { get; set; }
        public DateTime? FileLastWriteTime { get; set; }

        public override string ToString()
        {
            return InformationalVersion ?? FileVersion ?? AssemblyVersion?.ToString() ?? "Unknown Version";
        }
    }
}
