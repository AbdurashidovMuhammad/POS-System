namespace Application.DTOs.ReportDTOs;

public class SalesReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<SalesReportItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}
