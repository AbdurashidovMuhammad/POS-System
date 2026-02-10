using Application.Services;
using Application.Services.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoriesService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBarcodeService, BarcodeService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
