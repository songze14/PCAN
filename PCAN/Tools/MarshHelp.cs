using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Tools
{
    public static class MarshHelp
    {
        public static T ByteToStruct<T>(byte[] bytes, SizeTailEnum sizeTailEnum) where T : struct
        {
            var size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            if (size > bytes.Length)
            {
                throw new ArgumentException("字节数组太小，无法转换为指定的结构体类型.");
            }
            var ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            try
            {
                switch (sizeTailEnum)
                {
                    case SizeTailEnum.BigEndian:
                        break;
                    case SizeTailEnum.LittleEndian:
                        Array.Reverse(bytes, 0, bytes.Length);

                        break;
                    default:
                        break;
                }
                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, ptr, size);
                return (T)System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
            }
        }
        public static object ByteToStruct(byte[] bytes, Type type, SizeTailEnum sizeTailEnum)
        {
            var size = System.Runtime.InteropServices.Marshal.SizeOf(type);
            if (size > bytes.Length)
            {
                throw new ArgumentException("字节数组太小，无法转换为指定的结构体类型.");
            }
            var ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            try
            {
                switch (sizeTailEnum)
                {
                    case SizeTailEnum.BigEndian:
                        break;
                    case SizeTailEnum.LittleEndian:
                        Array.Reverse(bytes, 0, bytes.Length);

                        break;
                    default:
                        break;
                }
                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, ptr, size);
                return System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, type);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
            }
        }


        public enum SizeTailEnum
        {
            BigEndian,
            LittleEndian
        }
    }
}
