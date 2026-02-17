using Application.DTOs;
using Application.DTOs.ProductDTOs;
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

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
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
