using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WPF.Services;
using WPF.ViewModels;

namespace WPF;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var services = new ServiceCollection();
        ConfigureServices(services, config);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        // HttpClient - singleton uchun
        services.AddSingleton(sp =>
        {
            var baseUrl = config["ApiBaseUrl"]!;
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
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
        services.AddTransient<AdminDashboardViewModel>();
        services.AddTransient<ProductViewModel>();
        services.AddTransient<SalesViewModel>();
        services.AddTransient<ReportViewModel>();
        services.AddTransient<CategoryViewModel>();
        services.AddTransient<UserViewModel>();
        services.AddTransient<ActivityLogViewModel>();
        services.AddTransient<SalesHistoryViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Token refresh callback'ni ulash
        var apiService = _serviceProvider.GetRequiredService<IApiService>();
        var authService = _serviceProvider.GetRequiredService<IAuthService>();
        apiService.OnUnauthorized = authService.RefreshTokenAsync;

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

        mainWindow.DataContext = mainViewModel;
        mainWindow.Show();

        mainViewModel.Initialize();
    }
}
