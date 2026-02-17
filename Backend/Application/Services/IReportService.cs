using Application.DTOs.Common;
using Application.DTOs.ReportDTOs;

namespace Application.Services;

public interface IReportService
{
    Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, PaginationParams pagination);
    Task<StockInReportDto> GetStockInReportAsync(DateTime from, DateTime to, PaginationParams pagination);
    Task<byte[]> ExportSalesReportAsync(DateTime from, DateTime to);
    Task<byte[]> ExportStockInReportAsync(DateTime from, DateTime to);
}
