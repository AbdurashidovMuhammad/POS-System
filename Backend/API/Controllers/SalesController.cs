using Application.DTOs;
using Application.DTOs.Common;
using Application.DTOs.ProductDTOs;
using Application.DTOs.ReportDTOs;
using Application.DTOs.SaleDTOs;
using Application.Exceptions;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly IReportService _reportService;

    public SalesController(ISaleService saleService, IReportService reportService)
    {
        _saleService = saleService;
        _reportService = reportService;
    }

    /// <summary>
    /// Create a new sale (chek yaratish)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResult<SaleDto>.Failure(errors));
            }

            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResult<SaleDto>.Failure(["Foydalanuvchi autentifikatsiyadan o'tmagan"]));
            }

            var sale = await _saleService.CreateSaleAsync(dto, userId);
            return Ok(ApiResult<SaleDto>.Success(sale));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResult<SaleDto>.Failure([ex.Message]));
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(ApiResult<SaleDto>.Failure([ex.Message]));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResult<SaleDto>.Failure([ex.Message]));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<SaleDto>.Failure([ex.Message]));
        }
    }

    /// <summary>
    /// Get current user's sales history with pagination and date range filter
    /// </summary>
    [HttpGet("my-history")]
    public async Task<IActionResult> GetMySalesHistory(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] PaginationParams pagination)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResult<SalesReportDto>.Failure(["Foydalanuvchi autentifikatsiyadan o'tmagan"]));
            }

            if (from == default || to == default)
                return BadRequest(ApiResult<SalesReportDto>.Failure(["Sana kiritilishi shart"]));

            if (from.Date > to.Date)
                return BadRequest(ApiResult<SalesReportDto>.Failure(["Boshlanish sanasi tugash sanasidan katta bo'lishi mumkin emas"]));

            var report = await _reportService.GetSalesReportAsync(from, to, pagination, userId);
            return Ok(ApiResult<SalesReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<SalesReportDto>.Failure([ex.Message]));
        }
    }

    [HttpGet("top-selling")]
    public async Task<IActionResult> GetTopSellingProducts()
    {
        try
        {
            var products = await _saleService.GetTopSellingProductsAsync();
            return Ok(ApiResult<List<ProductDto>>.Success(products));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<List<ProductDto>>.Failure([ex.Message]));
        }
    }
}
