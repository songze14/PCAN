using PCAN.Shard.Modles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        public static List<ClassCToDotNetTypeInfo> TypeInfos { get; set; } =
       [
           new ClassCToDotNetTypeInfo(){Name="u8",TargetType=typeof(byte),FullName=typeof(byte).FullName,Size=Marshal.SizeOf(typeof(byte))},
            new ClassCToDotNetTypeInfo(){Name="u16",TargetType=typeof(ushort),FullName=typeof(ushort).FullName,Size=Marshal.SizeOf(typeof(ushort))},
            new ClassCToDotNetTypeInfo(){Name="u32",TargetType=typeof(uint),FullName=typeof(uint).FullName,Size=Marshal.SizeOf(typeof(uint))},
            new ClassCToDotNetTypeInfo(){Name="u64",TargetType=typeof(ulong),FullName=typeof(ulong).FullName,Size=Marshal.SizeOf(typeof(ulong))},
            new ClassCToDotNetTypeInfo(){Name="s8",TargetType=typeof(sbyte),FullName=typeof(sbyte).FullName,Size=Marshal.SizeOf(typeof(sbyte))},
            new ClassCToDotNetTypeInfo(){Name="s16",TargetType=typeof(short),FullName=typeof(short).FullName,Size=Marshal.SizeOf(typeof(short))},
            new ClassCToDotNetTypeInfo(){Name="s32",TargetType=typeof(int),FullName=typeof(int).FullName,Size=Marshal.SizeOf(typeof(int))},
            new ClassCToDotNetTypeInfo(){Name="s64",TargetType=typeof(long),FullName=typeof(long).FullName,Size=Marshal.SizeOf(typeof(long))},
            new ClassCToDotNetTypeInfo(){Name="float",TargetType=typeof(float),FullName=typeof(float).FullName,Size=Marshal.SizeOf(typeof(float))},
            new ClassCToDotNetTypeInfo(){Name="char",TargetType=typeof(char),FullName=typeof(char).FullName,Size=Marshal.SizeOf(typeof(char))},
        ];
    }
}
