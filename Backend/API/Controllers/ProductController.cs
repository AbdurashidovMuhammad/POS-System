using System.Security.Claims;
using Application.Authorization;
using Application.DTOs;
using Application.DTOs.Common;
using Application.DTOs.ProductDTOs;
using Application.Exceptions;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IBarcodeService _barcodeService;

    public ProductController(IProductService productService, IBarcodeService barcodeService)
    {
        _productService = productService;
        _barcodeService = barcodeService;
    }

    [HttpGet]
    [HasPermission("Products", "Read")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
    {
        try
        {
            var result = await _productService.GetAllProductsAsync(paginationParams);
            return Ok(ApiResult<PagedResult<ProductDto>>.Success(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<PagedResult<ProductDto>>.Failure([ex.Message]));
        }
    }

    [HttpGet("{id:int}")]
    [HasPermission("Products", "Read")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(ApiResult<ProductDto>.Success(product));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResult<ProductDto>.Failure([ex.Message]));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<ProductDto>.Failure([ex.Message]));
        }
    }

    [HttpGet("search")]
    [HasPermission("Products", "Read")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        try
        {
            var suggestions = await _productService.SearchProductsByNameAsync(query);
            return Ok(ApiResult<List<ProductSuggestDto>>.Success(suggestions.ToList()));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<List<ProductSuggestDto>>.Failure([ex.Message]));
        }
    }

    [HttpGet("search/full")]
    [HasPermission("Products", "Read")]
    public async Task<IActionResult> SearchFull([FromQuery] string query, [FromQuery] PaginationParams paginationParams)
    {
        try
        {
            var result = await _productService.SearchProductsFullAsync(query, paginationParams);
            return Ok(ApiResult<PagedResult<ProductDto>>.Success(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<PagedResult<ProductDto>>.Failure([ex.Message]));
        }
    }

    [HttpGet("barcode/{barcode}")]
    [HasPermission("Products", "Read")]
    public async Task<IActionResult> GetByBarcode(string barcode)
    {
        try
        {
            var product = await _productService.GetProductByBarcodeAsync(barcode);
            return Ok(ApiResult<ProductDto>.Success(product));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResult<ProductDto>.Failure([ex.Message]));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<ProductDto>.Failure([ex.Message]));
        }
    }

    [HttpPost]
    [HasPermission("Products", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResult<int>.Failure(errors));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim?.Value, out var userId);

            var productId = await _productService.CreateProductAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = productId }, ApiResult<int>.Success(productId));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResult<int>.Failure([ex.Message]));
        }
        catch (DuplicateException ex)
        {
            return Conflict(ApiResult<int>.Failure([ex.Message]));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResult<int>.Failure([ex.Message]));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<int>.Failure([ex.Message]));
        }
    }

    [HttpPut("{id:int}")]
    [HasPermission("Products", "Update")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResult<ProductDto>.Failure(errors));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim?.Value, out var userId);

            var product = await _productService.UpdateProductAsync(id, dto, userId);
            return Ok(ApiResult<ProductDto>.Success(product));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResult<ProductDto>.Failure([ex.Message]));
        }
        catch (DuplicateException ex)
        {
            return Conflict(ApiResult<ProductDto>.Failure([ex.Message]));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResult<ProductDto>.Failure([ex.Message]));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<ProductDto>.Failure([ex.Message]));
        }
    }

    [HttpPost("{id:int}/stock")]
    [HasPermission("Products", "AddStock")]
    public async Task<IActionResult> AddStock(int id, [FromBody] AddStockDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResult<ProductDto>.Failure(errors));
            }

            var product = await _productService.AddStockAsync(id, dto);
            return Ok(ApiResult<ProductDto>.Success(product));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResult<ProductDto>.Failure([ex.Message]));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<ProductDto>.Failure([ex.Message]));
        }
    }

    [HttpDelete("{id:int}")]
    [HasPermission("Products", "Delete")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim?.Value, out var userId);

            await _productService.DeactivateProductAsync(id, userId);
            return Ok(ApiResult<string>.Success("Mahsulot muvaffaqiyatli o'chirildi"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResult<string>.Failure([ex.Message]));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<string>.Failure([ex.Message]));
        }
    }

    [HttpGet("by-category/{categoryId:int}")]
    [HasPermission("Products", "Read")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(ApiResult<List<ProductDto>>.Success(products));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<List<ProductDto>>.Failure([ex.Message]));
        }
    }

    /// <summary>
    /// Get FIFO batches for a product (SuperAdmin only)
    /// </summary>
    [HttpGet("{id:int}/batches")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetBatches(int id)
    {
        try
        {
            var batches = await _productService.GetBatchesAsync(id);
            return Ok(ApiResult<List<ProductBatchDto>>.Success(batches));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<List<ProductBatchDto>>.Failure([ex.Message]));
        }
    }

    [HttpGet("{id:int}/barcode-image")]
    [HasPermission("Products", "Read")]
    public async Task<IActionResult> GetBarcodeImage(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            var imageBytes = await _barcodeService.GenerateBarcodeImageAsync(
                product.Barcode,
                product.Name,
                product.SellPrice);

            return File(imageBytes, "image/png", $"barcode_{product.Barcode}.png");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResult<byte[]>.Failure([ex.Message]));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<byte[]>.Failure([ex.Message]));
        }
    }
}
