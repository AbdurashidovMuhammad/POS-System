namespace Application.DTOs.ReportDTOs;

public class OrdersReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<OrderReportItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}

public class OrderReportItemDto
{
    public int SaleId { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentTypeName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public int ItemCount { get; set; }
    public List<OrderReportProductDto> Items { get; set; } = new();
}

public class OrderReportProductDto
{
    public string ProductName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public string FormattedQuantity => Quantity.ToString("0.##");
    public string UnitTypeName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
