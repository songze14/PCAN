using Excel.Tool;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN_AutoCar_Test_Client.Models
{
    public class TestExcelGrid:ReactiveObject
    {
        [Reactive]
        public int Index { get; set; }
        [Reactive]
        public string SendId { get; set; }
      
        [Reactive]
        public string SendData { get; set; }
        [Reactive]
        public string ParmName { get; set; }

        public string RecvId { get; set; }
        public string RecvCommandId { get; set; }
        /// <summary>
        /// 回复开始解析数据
        /// </summary>
        public int RecvBeDataIndex { get; set; }
        /// <summary>
        /// 回复结束解析数据
        /// </summary>
        public int RecvEnDataIndex { get; set; }
        [Reactive]
        public string RecvData { get; set; }

        public string DataType { get; set; } = string.Empty;
       
        [Reactive]
        public string MinData { get; set; } = string.Empty;
       
        [Reactive]
        public string MaxData { get; set; } = string.Empty;
        [Reactive]
        public TestPassEnum Pass { get; set; } = TestPassEnum.Non;
        public int 帧间隔 { get; set; }
    }
}
