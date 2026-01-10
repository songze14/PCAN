using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PCAN.Shard.Tools
{
    public static partial class ParmRegex
    {
        [GeneratedRegex(@"(?<=_\s*).*?(?=\s*,\()")]
        public static partial Regex TypeRegex();
        [GeneratedRegex(@"(?<=&\(\s*).*?(?=\s*\))")]
        public static partial Regex NameRegex();
        [GeneratedRegex(@"(?<=//(\s*))[\w.]+")]
        public static partial Regex RemarkRegex();
        [GeneratedRegex(@"(\b(?:u?int(?:8|16|32|64)?|u8|u16|u32|s8|s16|s32|float|double|char)\b)")]
        public static partial Regex DeviceTypeRegex();
        [GeneratedRegex(@"//\s*(.*)")]
        public static partial Regex DeviceRemarkRegex();

        [GeneratedRegex(@"(?<=\b(?:u8|u16|u32|s8|s16|s32|char|float|double)\s+).*(?=\s*;)")]
        public static partial Regex DeviceNameRegex();
        [GeneratedRegex(@"(?<=\[)(\d+)(?=\])")]
        public static partial Regex DeviceIsArry();

    }
}
