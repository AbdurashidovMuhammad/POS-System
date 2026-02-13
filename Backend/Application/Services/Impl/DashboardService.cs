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

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(int? userId = null)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        var salesQuery = _context.Sales.AsNoTracking();
        if (userId.HasValue)
        {
            salesQuery = salesQuery.Where(s => s.UserId == userId.Value);
        }

        var todaySalesQuery = salesQuery
            .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow);

        var todaySalesAmount = await todaySalesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        var todayOrdersCount = await todaySalesQuery.CountAsync();

        var yesterdaySalesAmount = await salesQuery
            .Where(s => s.SaleDate >= yesterday && s.SaleDate < today)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;

        var totalProductsCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.IsActive);

        var totalCategoriesCount = await _context.Categories
            .AsNoTracking()
            .CountAsync(c => c.IsActive);

        return new DashboardStatsDto
        {
            TodaySalesAmount = todaySalesAmount,
            YesterdaySalesAmount = yesterdaySalesAmount,
            TodayOrdersCount = todayOrdersCount,
            TotalProductsCount = totalProductsCount,
            TotalCategoriesCount = totalCategoriesCount
        };
    }

    public async Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        // Umumiy bugungi sotish
        var todaySalesQuery = _context.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow);

        var todaySalesAmount = await todaySalesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        var todayOrdersCount = await todaySalesQuery.CountAsync();

        // Kechagi sotish
        var yesterdaySalesQuery = _context.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= yesterday && s.SaleDate < today);

        var yesterdaySalesAmount = await yesterdaySalesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        var yesterdayOrdersCount = await yesterdaySalesQuery.CountAsync();

        // Mahsulotlar va kategoriyalar
        var totalProductsCount = await _context.Products.AsNoTracking().CountAsync(p => p.IsActive);
        var totalCategoriesCount = await _context.Categories.AsNoTracking().CountAsync(c => c.IsActive);

        // Top 5 mahsulot bugun
        var topSellingProducts = await _context.SaleItems
            .AsNoTracking()
            .Where(si => si.Sale.SaleDate >= today && si.Sale.SaleDate < tomorrow)
            .GroupBy(si => new { si.ProductId, si.Product.Name })
            .Select(g => new TopSellingProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(si => si.Quantity),
                TotalRevenue = g.Sum(si => si.Quantity * si.UnitPrice)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToListAsync();

        // Kassirlar bo'yicha sotish bugun
        var cashierSales = await _context.Sales
            .AsNoTracking()
            .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
            .GroupBy(s => new { s.UserId, s.User.Username })
            .Select(g => new CashierSalesDto
            {
                UserId = g.Key.UserId,
                Username = g.Key.Username,
                TodaySalesAmount = g.Sum(s => s.TotalAmount),
                TodayOrdersCount = g.Count()
            })
            .OrderByDescending(x => x.TodaySalesAmount)
            .ToListAsync();

        // Kam qolgan mahsulotlar (10 va undan kam)
        var lowStockProducts = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity <= 10)
            .OrderBy(p => p.StockQuantity)
            .Take(10)
            .Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CategoryName = p.Category.Name,
                StockQuantity = p.StockQuantity
            })
            .ToListAsync();

        return new AdminDashboardStatsDto
        {
            TodaySalesAmount = todaySalesAmount,
            YesterdaySalesAmount = yesterdaySalesAmount,
            TodayOrdersCount = todayOrdersCount,
            YesterdayOrdersCount = yesterdayOrdersCount,
            TotalProductsCount = totalProductsCount,
            TotalCategoriesCount = totalCategoriesCount,
            TopSellingProducts = topSellingProducts,
            CashierSales = cashierSales,
            LowStockProducts = lowStockProducts
        };
    }
}
