using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Shard.Tools
{
    public static class CTypeToCsharpTypeValue
    {
        public static string GetParmValue(string typename, byte[] data) => typename switch
        {
            "u8" => data[0].ToString(),
            "u16" => BitConverter.ToUInt16(data).ToString(),
            "u32" => BitConverter.ToUInt32(data).ToString(),
            "u64" => BitConverter.ToUInt64(data).ToString(),
            "s8" => Convert.ToSByte(data).ToString(),
            "s16" => BitConverter.ToInt16(data).ToString(),
            "s32" => BitConverter.ToInt32(data).ToString(),
            "s64" => BitConverter.ToUInt64(data).ToString(),
            "float" => BitConverter.ToSingle(data).ToString(),
            "char" => Encoding.ASCII.GetString(data),
            _ => throw new NotImplementedException(),
        };
    }
}
