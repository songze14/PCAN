using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN_AutoCar_Test_Client.Models
{
    public class Repetitiveinstructions
    {
        public RepetitiveInstruction Upload { get; set; }
        public RepetitiveInstruction LinTest { get; set; }
    }
    public class RepetitiveInstruction
    {
        /// <summary>
        /// 发送ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 发送数据
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 是否启用扩展帧
        /// </summary>
        public bool Extended { get; set; }
        /// <summary>
        /// 回复ID
        /// </summary>
        public string ReciveId { get; set; }
        /// <summary>
        /// 回复OK数据（全量比较）
        /// </summary>
        public string ReciveOkData { get; set; }
        /// <summary>
        /// 回复Ng数据（全量比较）
        /// </summary>
        public string ReciveNgData { get; set; }

        public int SendCount { get; set; }
        public int SendDelay { get; set; }
    }
}
