using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PCAN_AutoCar_Test_Client.ViewModel;
using ReactiveUI;
using Serilog;
using Splat.Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace PCAN_AutoCar_Test_Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Mutex _singletonMutex;
        public IHost _host { get; private set; }
        public IServiceProvider RootServiceProvider { get; internal set; }
        private CancellationTokenSource cts = new CancellationTokenSource();
        public App()
        {
            var appname = typeof(App).AssemblyQualifiedName;
            this._singletonMutex = new Mutex(true, appname, out var createdNew);
            InitHost();
        }
        private void InitHost()
        {
            this._host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder
                        .SetBasePath(context.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        
                    
                    builder.AddEnvironmentVariables();
                })
                .UseSerilog((hostingContext, services, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                )
                .ConfigureServices(HostStartup.ConfigureServices)
                .Build();
            this.RootServiceProvider = this._host.Services;
            RootServiceProvider.UseMicrosoftDependencyResolver();
        }
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            try
            {
                var mainWin = this.RootServiceProvider.GetRequiredService<IViewFor<MainWindowViewModel>>() as Window;
                mainWin.Show();
                var thread = new Thread(async () =>
                {
                    try
                    {
                        await _host.RunAsync(cts.Token);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        this.Shutdown();
                    }
                });
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException +"\r\n"+ex.StackTrace+"\r\n"+ ex.Message);
                using (var scope = this.RootServiceProvider.CreateScope())
                {
                    var sp = scope.ServiceProvider;
                    var logger = sp.GetRequiredService<ILogger<App>>();
                    logger.LogError($"{ex.Message}\r\n{ex.StackTrace}");
                }
                this.Shutdown();
            }

        }
    }

}
