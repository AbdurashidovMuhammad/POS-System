using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WPF.Services;
using WPF.ViewModels;

namespace WPF;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // HttpClient - singleton uchun
        services.AddSingleton(sp =>
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7144/")
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        });

        // Services - ApiService ham singleton bo'lishi kerak token saqlash uchun
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IAuthService, AuthService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductViewModel>();
        services.AddTransient<SalesViewModel>();
        services.AddTransient<ReportViewModel>();
        services.AddTransient<CategoryViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

        mainWindow.DataContext = mainViewModel;
        mainWindow.Show();

        mainViewModel.Initialize();
    }
}
