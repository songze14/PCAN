using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Shard.Tools
{
    public static class MathTool
    {
        /// <summary>
        /// 将字节长度转换为 CAN FD DLC 值
        /// </summary>
        /// <param name="dataLength">实际数据长度 (0-64)</param>
        /// <returns>对应的 DLC 代码</returns>
        public static byte GetDLCFromLength(int dataLength)
        {
            // 1. 0-8 字节直接对应
            if (dataLength <= 8) return (byte)dataLength;

            // 2. 大于 8 字节的阶梯映射
            if (dataLength <= 12) return 9;
            if (dataLength <= 16) return 10;
            if (dataLength <= 20) return 11;
            if (dataLength <= 24) return 12;
            if (dataLength <= 32) return 13;
            if (dataLength <= 48) return 14;
            if (dataLength <= 64) return 15;

            // 3. 超出范围（通常抛出异常或截断）
            throw new ArgumentOutOfRangeException("CAN FD 最大只支持 64 字节");
        }
    }
}
