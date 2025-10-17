using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PCAN.Notification.Log;

using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace PCAN_AutoCar_Test_Client
{
    internal static class HostStartup
    {
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.UseMicrosoftDependencyResolver();
            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();
          




        }

      
    }
}
