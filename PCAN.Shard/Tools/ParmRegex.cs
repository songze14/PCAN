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
    }
}
