using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Shard.Tools
{
    public static class DataTimeEX
    {
        /// <summary>
        /// 获取1970至现在的UTC秒数
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long  Get1970ToNowSeconds(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        /// <summary>
        /// 获取1970至现在的UTC毫秒数
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long Get1970ToNowMilliseconds(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
    }
}
