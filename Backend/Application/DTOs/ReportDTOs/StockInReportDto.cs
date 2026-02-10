namespace Application.DTOs.ReportDTOs;

public class StockInReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<StockInReportItemDto> Items { get; set; } = new();
    public decimal TotalQuantity { get; set; }
}
