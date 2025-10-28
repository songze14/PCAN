
using Peak.Can.Basic;

namespace ConsoleAppTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var canFd = new CanFdCommunicator();

            if (canFd.Initialize())
            {
                // 发送CAN FD消息
                byte[] data = new byte[12]; // CAN FD支持最多64字节
                data[0] = 1;

                canFd.SendCanFdMessage(0x6280004, data);

                // 启动接收线程
                var receiveThread = new System.Threading.Thread(() =>
                {
                    canFd.ReceiveMessages();
                });
                receiveThread.Start();

                Console.WriteLine("按任意键退出...");
                Console.ReadKey();

                canFd.Close();
            }

        }
    }
}
