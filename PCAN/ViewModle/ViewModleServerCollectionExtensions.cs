using Microsoft.Extensions.DependencyInjection;
using PCAN.Modles;
using PCAN.View.RealtimePage;
using PCAN.View.UserPage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PCAN.ViewModle
{
    public static class ViewModleServerCollectionExtensions
    {
        public static IServiceCollection AddViewModles(this IServiceCollection services)
        {
            services.AddSingleton<BasicFunctionsPageViewModel>();
            services.AddSingleton<ParmValueSettingPageViewModel>();
            services.AddSingleton<AppViewModle>(sp =>
            {
                var appvm = new AppViewModle(sp);
                appvm.MapSourceToPage=url => url switch 
                {
                    UrlDefines.URL_BasicFunctions => sp.GetRequiredService<IViewFor<BasicFunctionsPageViewModel>>() as Page,
                    UrlDefines.URL_PCANDataParse => sp.GetRequiredService<IViewFor<ParmValueSettingPageViewModel>>() as Page,
                }
                ;
                return appvm;
            });
            return services;
        }
    }
}
