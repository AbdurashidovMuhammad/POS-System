namespace WPF.Models;

public class SalesReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<SalesReportItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class SalesReportItemDto
{
    public DateTime Date { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
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
}

public class StockInReportItemDto
{
    public DateTime Date { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitTypeName { get; set; } = string.Empty;
}
