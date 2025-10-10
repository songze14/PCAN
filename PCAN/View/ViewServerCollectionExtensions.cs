using Microsoft.Extensions.DependencyInjection;
using PCAN.View.RealtimePage;
using PCAN.View.UserPage;
using PCAN.View.Windows;
using PCAN.ViewModel;
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
            services.AddSingleton<IViewFor<BasicFunctionsPageViewModel>, BasicFunctionsPage>();
            services.AddSingleton<IViewFor<MainWindowViewModel>, MainWindow>();
            services.AddSingleton<IViewFor<ParmValueSettingPageViewModel>, ParmValueSettingPage>();
            return services;
        }
    }
}
