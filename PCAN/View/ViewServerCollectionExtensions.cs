using Microsoft.Extensions.DependencyInjection;
using PCAN.View.UserPage;
using PCAN.ViewModle;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN.View
{
    public static class ViewServerCollectionExtensions
    {
        public static IServiceCollection AddViews(this IServiceCollection services)
        {
            services.AddSingleton<IViewFor<RealtimePageViewModel>, RealtimePage>();
            services.AddSingleton<IViewFor<MainWindowViewModle>, MainWindow>();
            return services;
        }
    }
}
