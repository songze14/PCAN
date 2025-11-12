using Microsoft.Extensions.DependencyInjection;
using PCAN.UserControls;
using PCAN.View.RealtimePage;
using PCAN.View.UserPage;
using PCAN.ViewModel.RunPage;
using PCAN.ViewModel.USercontrols;
using PCAN.ViewModel.Window;
using ReactiveUI;

namespace PCAN.View
{
    public static class ViewServerCollectionExtensions
    {
        public static IServiceCollection AddViews(this IServiceCollection services)
        {
            services.AddSingleton<IViewFor<BasicFunctionsPageViewModel>, BasicFunctionsPage>();
            services.AddSingleton<IViewFor<MainWindowViewModel>, MainWindow>();
            services.AddSingleton<IViewFor<ParmValueSettingPageViewModel>, ParmValueSettingPage>();
            services.AddTransient<IViewFor<PCanClientUsercontrolViewModel>, PCanClientUsercontrol>();
            services.AddSingleton<IViewFor<UploadPageViewModel>, UploadPage>();
            services.AddSingleton<IViewFor<UILogsViewModel>, UILogsView>();
            services.AddSingleton<IViewFor<DeviceParmTuningPageViewModel>, DeviceParmTuningPage>();
            services.AddSingleton<IViewFor<DataMonitoringPageViewModel>, DataMonitoringPage>();
            services.AddSingleton<IViewFor<SysTemSettingsPageViewModel>, SysTemSettingsPage>();
            services.AddTransient<WpfPlotGLUserControl>();
            return services;
        }
    }
}
