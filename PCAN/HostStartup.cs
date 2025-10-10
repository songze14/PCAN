using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PCAN.Notification.Log;
using PCAN.View;
using PCAN.ViewModel;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace PCAN
{
    internal static class HostStartup
    {
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.UseMicrosoftDependencyResolver();
            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LogNotification).Assembly) );
            services.AddViewModels();
            services.AddViews();




        }

      
    }
}
