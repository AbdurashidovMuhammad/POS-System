using Application.DTOs.Common;
using Application.DTOs.ReportDTOs;

namespace Application.Services;

public interface IReportService
{
    Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, PaginationParams pagination, int? userId = null);
    Task<StockInReportDto> GetStockInReportAsync(DateTime from, DateTime to, PaginationParams pagination, int? userId = null);
    Task<byte[]> ExportSalesReportAsync(DateTime from, DateTime to, int? userId = null);
    Task<byte[]> ExportStockInReportAsync(DateTime from, DateTime to, int? userId = null);
}
