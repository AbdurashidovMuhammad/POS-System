namespace WPF.Models;

public class AdminDashboardStatsDto
{
    public decimal TodaySalesAmount { get; set; }
    public decimal YesterdaySalesAmount { get; set; }
    public int TodayOrdersCount { get; set; }
    public int YesterdayOrdersCount { get; set; }
    public int TotalProductsCount { get; set; }
    public int TotalCategoriesCount { get; set; }
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = [];
    public List<CashierSalesDto> CashierSales { get; set; } = [];
    public List<LowStockProductDto> LowStockProducts { get; set; } = [];
}

public class TopSellingProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class CashierSalesDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal TodaySalesAmount { get; set; }
    public int TodayOrdersCount { get; set; }
}

public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal StockQuantity { get; set; }
}
