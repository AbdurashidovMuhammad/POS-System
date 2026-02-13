using Application.DTOs.DashboardDTOs;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

internal class DashboardService : IDashboardService
{
    private readonly DatabaseContext _context;

    public DashboardService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var todaySalesQuery = _context.Sales
            .AsNoTracking()
            .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow);

        var todaySalesAmount = await todaySalesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        var todayOrdersCount = await todaySalesQuery.CountAsync();

        var totalProductsCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.IsActive);

        var totalCategoriesCount = await _context.Categories
            .AsNoTracking()
            .CountAsync(c => c.IsActive);

        return new DashboardStatsDto
        {
            TodaySalesAmount = todaySalesAmount,
            TodayOrdersCount = todayOrdersCount,
            TotalProductsCount = totalProductsCount,
            TotalCategoriesCount = totalCategoriesCount
        };
    }
}
