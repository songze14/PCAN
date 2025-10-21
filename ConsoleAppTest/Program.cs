using MediatR;
using PCAN.Drive;
using PCAN.Notification.Log;
using Peak.Can.Basic;

namespace ConsoleAppTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PcanStatus result;
            result = Api.Initialize(PcanChannel.Usb01, Bitrate.Pcan500);
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();

        }
    }
}
