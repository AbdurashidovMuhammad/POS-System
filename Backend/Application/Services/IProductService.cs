using Application.DTOs.Common;
using Application.DTOs.ProductDTOs;

namespace Application.Services;

public interface IProductService
{
    /// <summary>
    /// Get all active products with pagination
    /// </summary>
    Task<PagedResult<ProductDto>> GetAllProductsAsync(PaginationParams paginationParams);

    /// <summary>
    /// Get product by ID
    /// </summary>
    Task<ProductDto> GetProductByIdAsync(int id);

    /// <summary>
    /// Search products by name (autocomplete)
    /// </summary>
    Task<IEnumerable<ProductSuggestDto>> SearchProductsByNameAsync(string query);

    /// <summary>
    /// Full search products by name with pagination
    /// </summary>
    Task<PagedResult<ProductDto>> SearchProductsFullAsync(string query, PaginationParams paginationParams);

    /// <summary>
    /// Get product by barcode (for scanner input)
    /// </summary>
    Task<ProductDto> GetProductByBarcodeAsync(string barcode);

    /// <summary>
    /// Create new product with auto-generated barcode, returns product ID
    /// </summary>
    Task<int> CreateProductAsync(CreateProductDto dto, int userId);

    /// <summary>
    /// Update product information
    /// </summary>
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto, int userId);

    /// <summary>
    /// Add stock to product
    /// </summary>
    Task<ProductDto> AddStockAsync(int id, AddStockDto dto);

    /// <summary>
    /// Deactivate product (soft delete)
    /// </summary>
    Task DeactivateProductAsync(int id, int userId);

    /// <summary>
    /// Get products by category ID
    /// </summary>
    Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId);
}
