using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PCAN.Notification.Log;
using PCAN_AutoCar_Test_Client.Models;
using PCAN_AutoCar_Test_Client.Notification.Log;
using PCAN_AutoCar_Test_Client.View;
using PCAN_AutoCar_Test_Client.ViewModel;
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
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LogNotificationHandle).Assembly));
            services.AddViews();
            services.AddViewModels();




        }

      
    }
}
