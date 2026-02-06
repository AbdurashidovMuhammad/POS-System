using WPF.Enums;

namespace WPF.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public UnitType UnitType { get; set; }
    public decimal StockQuantity { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal UnitPrice { get; set; }
    public UnitType UnitType { get; set; }
    public decimal StockQuantity { get; set; }
    public string? Barcode { get; set; }
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public int? CategoryId { get; set; }
    public decimal? UnitPrice { get; set; }
    public UnitType? UnitType { get; set; }
    public bool? IsActive { get; set; }
}

public class AddStockDto
{
    public decimal Quantity { get; set; }
}
