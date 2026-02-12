namespace Application.DTOs.ReportDTOs;

public class StockInReportItemDto
{
    public DateTime Date { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public string UnitTypeName { get; set; } = null!;
    public string Username { get; set; } = null!;
}
