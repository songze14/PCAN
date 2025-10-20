using Microsoft.Extensions.DependencyInjection;
using PCAN_AutoCar_Test_Client.UserControls;
using PCAN_AutoCar_Test_Client.ViewModel;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN_AutoCar_Test_Client.View
{
    public static class ViewServerCollectionExtensions
    {
        public static IServiceCollection AddViews(this IServiceCollection services)
        {
            services.AddSingleton<IViewFor<MainWindowViewModel>, MainWindow>();
            services.AddSingleton<IViewFor<TestRealtimePageViewModel>, TestRealtimePage>();
            services.AddTransient<IViewFor<PCanClientUsercontrolViewModel>, PCanClientUsercontrol>();
            services.AddSingleton<IViewFor<UILogsViewModel>, UILogsView>();
            return services;
        }
    }
}
