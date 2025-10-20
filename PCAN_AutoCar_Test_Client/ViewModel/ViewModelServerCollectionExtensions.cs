using Microsoft.Extensions.DependencyInjection;
using PCAN_AutoCar_Test_Client.Models;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PCAN_AutoCar_Test_Client.ViewModel
{
    public static class ViewModelServerCollectionExtensions
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<AppViewModel>(sp =>
            {
                var appvm = new AppViewModel(sp);
                appvm.MapSourceToPage = url => url switch
                {
                    UrlDefines.URL_Test => sp.GetRequiredService<IViewFor<TestRealtimePageViewModel>>() as Page,
                  
                }
                ;
                return appvm;
            });
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<TestRealtimePageViewModel>();
            services.AddTransient<PCanClientUsercontrolViewModel>();
            services.AddSingleton<UILogsViewModel>();
            return services;

        }

    }
}
