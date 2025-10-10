using Microsoft.Extensions.DependencyInjection;
using PCAN.UserControls;
using PCAN.View.RealtimePage;
using PCAN.View.UserPage;
using PCAN.ViewModel;
using PCAN.ViewModel.USercontrols;
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
            return services;
        }
    }
}
