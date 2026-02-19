namespace Application.DTOs.ReportDTOs;

public class StockInReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<StockInReportItemDto> Items { get; set; } = new();
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
