using Application.DTOs.CategoryDTOs;
using Core.Enums;

namespace Application.DTOs.ProductDTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public decimal SellPrice { get; set; }
    public Unit_Type UnitType { get; set; }
    public decimal StockQuantity { get; set; }
    public string Barcode { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
