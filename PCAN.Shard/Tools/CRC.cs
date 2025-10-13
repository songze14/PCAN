using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Shard.Tools
{
    public static class CRC
    {
        public static byte CalculateCRC8(byte[] data)
        {
            byte crc = 0x00; // 初始值
            byte polynomial = 0x8C; // CRC-8 多项式

            foreach (byte b in data)
            {
                crc ^= b; // 按位异或
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x80) != 0) // 检查最高位
                    {
                        crc = (byte)((crc << 1) ^ polynomial); // 左移并异或多项式
                    }
                    else
                    {
                        crc <<= 1; // 左移一位
                    }
                }
            }

            return crc;
        }

    }
}
