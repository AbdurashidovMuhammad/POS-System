namespace Application.DTOs.ReportDTOs;

public class SalesReportItemDto
{
    public DateTime Date { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public string UnitTypeName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
