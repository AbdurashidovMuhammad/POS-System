using Application.DTOs;
using Application.DTOs.Common;
using Application.DTOs.ReportDTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "SuperAdmin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] PaginationParams pagination, [FromQuery] int? userId = null)
    {
        try
        {
            var validationError = ValidateDateRange(from, to);
            if (validationError is not null)
                return BadRequest(ApiResult<SalesReportDto>.Failure([validationError]));

            var report = await _reportService.GetSalesReportAsync(from, to, pagination, userId);
            return Ok(ApiResult<SalesReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<SalesReportDto>.Failure([ex.Message]));
        }
    }

    [HttpGet("stock-in")]
    public async Task<IActionResult> GetStockInReport([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] PaginationParams pagination, [FromQuery] int? userId = null)
    {
        try
        {
            var validationError = ValidateDateRange(from, to);
            if (validationError is not null)
                return BadRequest(ApiResult<StockInReportDto>.Failure([validationError]));

            var report = await _reportService.GetStockInReportAsync(from, to, pagination, userId);
            return Ok(ApiResult<StockInReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<StockInReportDto>.Failure([ex.Message]));
        }
    }

    [HttpGet("sales/export")]
    public async Task<IActionResult> ExportSalesReport([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? userId = null)
    {
        try
        {
            var validationError = ValidateDateRange(from, to);
            if (validationError is not null)
                return BadRequest(ApiResult<string>.Failure([validationError]));

            var fileBytes = await _reportService.ExportSalesReportAsync(from, to, userId);
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Sotilgan_mahsulotlar_{from:dd.MM.yyyy}-{to:dd.MM.yyyy}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<string>.Failure([$"Excel yaratishda xatolik: {ex.Message}"]));
        }
    }

    [HttpGet("stock-in/export")]
    public async Task<IActionResult> ExportStockInReport([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? userId = null)
    {
        try
        {
            var validationError = ValidateDateRange(from, to);
            if (validationError is not null)
                return BadRequest(ApiResult<string>.Failure([validationError]));

            var fileBytes = await _reportService.ExportStockInReportAsync(from, to, userId);
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Kirim_mahsulotlar_{from:dd.MM.yyyy}-{to:dd.MM.yyyy}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<string>.Failure([$"Excel yaratishda xatolik: {ex.Message}"]));
        }
    }

    private static string? ValidateDateRange(DateTime from, DateTime to)
    {
        if (from == default || to == default)
            return "Sana kiritilishi shart";

        if (from.Date > to.Date)
            return "Boshlanish sanasi tugash sanasidan katta bo'lishi mumkin emas";

        if (from.Date > DateTime.Now.Date)
            return "Kelajakdagi sana bo'lishi mumkin emas";

        return null;
    }
}
