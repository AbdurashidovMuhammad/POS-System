using Application.DTOs.ProductDTOs;
using Application.DTOs.SaleDTOs;

namespace Application.Services;

public interface ISaleService
{
    /// <summary>
    /// Create a new sale with items, update stock, and record stock movements
    /// </summary>
    /// <param name="dto">Sale creation data with items</param>
    /// <param name="userId">User ID from JWT token</param>
    /// <returns>Created sale with items</returns>
    Task<SaleDto> CreateSaleAsync(CreateSaleDto dto, int userId);

    /// <summary>
    /// Buyurtmani ID bo'yicha olish
    /// </summary>
    Task<SaleDto?> GetSaleByIdAsync(int id);

    /// <summary>
    /// Bugungi eng ko'p sotilgan mahsulotlarni qaytaradi
    /// </summary>
    Task<List<ProductDto>> GetTopSellingProductsAsync(int count = 5);
}
