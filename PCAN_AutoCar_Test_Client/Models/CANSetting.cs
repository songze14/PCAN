using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN_AutoCar_Test_Client.Models
{
    public class CANSetting
    {
        public bool UseFD { get; set; }
        public List<FDConStr> FDConStrEx { get; set; }
    }
    public class FDConStr
    {
        public string Name { get; set; }
        public string ConStr { get; set; }
    }
}
