using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.Shard.Modles
{
    public class ClassCToDotNetTypeInfo
    {

        public string Name { get; set; }
        public Type TargetType { get; set; }
        public string FullName { get; set; }
        public int Size { get; set; }
    }
}
