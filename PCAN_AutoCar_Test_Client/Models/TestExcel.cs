using Excel.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN_AutoCar_Test_Client.Models
{
    public class TestExcel
    {
        [EntityToexcel("发送命令/0x16进制", 1)]
        [ExcelToEntity(1)]
        public string SendId { get; set; }
        [EntityToexcel("发送数据/1byte用-隔开",2)]
        [ExcelToEntity(2)]
        public string SendData { get; set; }
        [EntityToexcel("参数名称", 3)]
        [ExcelToEntity(3)]
        public string ParmName { get; set; }
        [EntityToexcel("回复命令/0x16进制", 4)]
        [ExcelToEntity(4)]
        public string RecvId { get; set; }
        [EntityToexcel("回复子命令/0x16进制", 5)]
        [ExcelToEntity(5)]
        public string RecvCommandId { get; set; }
        /// <summary>
        /// 回复开始解析数据
        /// </summary>
        [EntityToexcel("回复解析开始位",6)]
        [ExcelToEntity(6)]
        public int RecvBeDataIndex { get; set; }
        /// <summary>
        /// 回复结束解析数据
        /// </summary>
        [EntityToexcel("回复解析结束位", 7)]
        [ExcelToEntity(7)]
        public int RecvEnDataIndex { get; set; }
        [EntityToexcel("回复解析数据类型", 8)]
        [ExcelToEntity(8)]
        public string DataType { get; set; }=string.Empty;
        [EntityToexcel("最小值/允许相等", 9)]
        [ExcelToEntity(9)]
        public string MinData { get; set; } = string.Empty;
        [EntityToexcel("最大值/允许相等", 10)]
        [ExcelToEntity(10)]
        public string MaxData { get; set; } = string.Empty;

        [EntityToexcel("帧间隔/单位ms", 11)]
        [ExcelToEntity(11)]
        public int 帧间隔 { get; set; } 
    }
}
