namespace WPF.Models;

public class SalesReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<SalesReportItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public decimal TotalProfit { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int OrderCount { get; set; }
    public int TotalPages { get; set; }
}

public class SalesReportItemDto
{
    public int SaleId { get; set; }
    public DateTime Date { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string FormattedQuantity => Quantity.ToString("0.##");
    public string UnitTypeName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal BuyPriceAtSale { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Profit { get; set; }
    public string PaymentTypeName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool DateGroupIsAlternate { get; set; }
}

public class OrdersReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<OrderReportItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class OrderReportItemDto
{
    public int SaleId { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentTypeName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public List<OrderReportProductDto> Items { get; set; } = new();
    public bool DateGroupIsAlternate { get; set; }
}

public class OrderReportProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string FormattedQuantity => Quantity.ToString("0.##");
    public string UnitTypeName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class StockInReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<StockInReportItemDto> Items { get; set; } = new();
    public decimal TotalQuantity { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class StockInReportItemDto
{
    public DateTime Date { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string FormattedQuantity => Quantity.ToString("0.##");
    public string UnitTypeName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool DateGroupIsAlternate { get; set; }
}
