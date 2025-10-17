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
            TPCANChannelInformation handle;
            uint iChannelsCount;
            var stsResult = PCANBasic.GetValue(PCANBasic.PCAN_NONEBUS, TPCANParameter.PCAN_ATTACHED_CHANNELS_COUNT, out iChannelsCount, sizeof(uint));
            if (stsResult == TPCANStatus.PCAN_ERROR_OK)
            {
                TPCANChannelInformation[] info = new TPCANChannelInformation[iChannelsCount];

                stsResult = PCANBasic.GetValue(PCANBasic.PCAN_NONEBUS, TPCANParameter.PCAN_ATTACHED_CHANNELS, info);
                if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                {
                    handle = info[0];
                    var can = new CANDrive(handle.channel_handle, 0x77, BitrateFD.BitrateSaeJ2284_4, 10,useFD: true);
                    can.CANReadMsg.Subscribe(msg =>
                    {
                        Console.WriteLine($"ID:{msg.ID:X},Type:{msg.MSGTYPE},Len:{msg.LEN},Data:{BitConverter.ToString(msg.DATA)}");
                    });

                }
            }
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();

        }
    }
}
