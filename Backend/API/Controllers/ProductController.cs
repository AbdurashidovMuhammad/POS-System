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
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IBarcodeService _barcodeService;

    public ProductController(IProductService productService, IBarcodeService barcodeService)
    {
        _productService = productService;
        _barcodeService = barcodeService;
    }

    /// <summary>
    /// Get all active products with pagination
    /// </summary>
    [HttpGet]
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

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:int}")]
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

    /// <summary>
    /// Search products by name (autocomplete)
    /// </summary>
    [HttpGet("search")]
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

    /// <summary>
    /// Full search products by name or barcode with pagination
    /// </summary>
    [HttpGet("search/full")]
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

    /// <summary>
    /// Get product by barcode (for scanner)
    /// </summary>
    [HttpGet("barcode/{barcode}")]
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

    /// <summary>
    /// Create new product, returns product ID
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResult<int>.Failure(errors));
            }

            var productId = await _productService.CreateProductAsync(dto);
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

    /// <summary>
    /// Update product
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResult<ProductDto>.Failure(errors));
            }

            var product = await _productService.UpdateProductAsync(id, dto);
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

    /// <summary>
    /// Add stock to product
    /// </summary>
    [HttpPost("{id:int}/stock")]
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

    /// <summary>
    /// Deactivate product (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _productService.DeactivateProductAsync(id);
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

    /// <summary>
    /// Get barcode image for product (PNG with name, barcode, price)
    /// </summary>
    [HttpGet("{id:int}/barcode-image")]
    public async Task<IActionResult> GetBarcodeImage(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            var imageBytes = await _barcodeService.GenerateBarcodeImageAsync(
                product.Barcode,
                product.Name,
                product.UnitPrice);

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
