using Microsoft.Extensions.DependencyInjection;
using PCAN.Modles;
using PCAN.ViewModel.RunPage;
using PCAN.ViewModel.USercontrols;
using PCAN.ViewModel.Window;
using ReactiveUI;
using System.Windows.Controls;

namespace PCAN.ViewModel
{
    public static class ViewModelServerCollectionExtensions
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<BasicFunctionsPageViewModel>();
            services.AddSingleton<ParmValueSettingPageViewModel>();
            services.AddSingleton<AppViewModel>(sp =>
            {
                var appvm = new AppViewModel(sp);
                appvm.MapSourceToPage=url => url switch 
                {
                    UrlDefines.URL_BasicFunctions => sp.GetRequiredService<IViewFor<BasicFunctionsPageViewModel>>() as Page,
                    UrlDefines.URL_PCANDataParse => sp.GetRequiredService<IViewFor<ParmValueSettingPageViewModel>>() as Page,
                    UrlDefines.URL_Upload => sp.GetRequiredService<IViewFor<UploadPageViewModel>>() as Page,
                    UrlDefines.URL_DeviceParmTuning => sp.GetRequiredService<IViewFor<DeviceParmTuningPageViewModel>>() as Page,
                    UrlDefines.URL_DataMonitoring => sp.GetRequiredService<IViewFor<DataMonitoringPageViewModel>>() as Page,
                }
                ;
                return appvm;
            });
            services.AddTransient<PCanClientUsercontrolViewModel>();
            services.AddSingleton<UploadPageViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<UILogsViewModel>();
            services.AddSingleton<DeviceParmTuningPageViewModel>();
            services.AddSingleton<DataMonitoringPageViewModel>();
            return services;
        }
    }
}
