using Peak.Can.Basic;
using Peak.Can.Basic.BackwardCompatibility;
using System;

namespace ConsoleAppTest
{
    

    public class CanFdCommunicator
    {
        private PcanChannel channel;
        private Bitrate fdBitrate;
        private Bitrate nominalBitrate;

        public CanFdCommunicator()
        {
            channel = PcanChannel.Usb01;
            nominalBitrate = Bitrate.Pcan1000;
            fdBitrate = Bitrate.Pcan500;
        }

        public bool Initialize()
        {
            try
            {
                // 初始化CAN FD通道
                var result = Api.Initialize(channel,new BitrateFD( BitrateFD.BitrateSaeJ2284_4));

                if (result != PcanStatus.OK)
                {
                    Console.WriteLine($"初始化失败: {result}");
                    return false;
                }

                Console.WriteLine("CAN FD通道初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化异常: {ex.Message}");
                return false;
            }
        }

        public void SendCanFdMessage(uint id, byte[] data)
        {
            try
            {
                var message = new PcanMessage(extendedDataLength:true)
                {
                    ID = id,
                    DLC = (byte)data.Length,
                    MsgType = MessageType.Extended,
                    Data = data,
                    
                };

                var result = Api.Write(channel,  message);

                if (result == PcanStatus.OK)
                {
                    Console.WriteLine($"CAN FD消息发送成功 - ID: 0x{id:X8}");
                }
                else
                {
                    Console.WriteLine($"发送失败: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送异常: {ex.Message}");
            }
        }

        public void ReceiveMessages()
        {
            try
            {
                PcanMessage message;
                ulong timestamp;

                while (true)
                {
                    var result = Api.Read(channel, out message,out timestamp);

                    if (result == PcanStatus.OK)
                    {
                        ProcessReceivedMessage(message, timestamp);
                    }
                    else if (result != PcanStatus.ReceiveQueueEmpty)
                    {
                        Console.WriteLine($"接收错误: {result}");
                        break;
                    }

                    System.Threading.Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接收异常: {ex.Message}");
            }
        }

        private void ProcessReceivedMessage(PcanMessage message, ulong timestamp)
        {
            Console.WriteLine($"收到CAN FD消息:");
            Console.WriteLine($"  ID: 0x{message.ID:X8}");
            Console.WriteLine($"  DLC: {message.DLC}");
            Console.WriteLine($"  数据: {BitConverter.ToString(message.Data, 0, message.DLC)}");
            Console.WriteLine($"  时间戳: {timestamp}");
        }

        public void Close()
        {
            Api.Uninitialize(channel);
            Console.WriteLine("CAN FD通道已关闭");
        }
    }
}
