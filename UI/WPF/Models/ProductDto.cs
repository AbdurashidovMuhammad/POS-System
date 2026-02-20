using WPF.Enums;

namespace WPF.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public decimal SellPrice { get; set; }
    public UnitType UnitType { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal MinStockThreshold { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsLowStock => MinStockThreshold > 0 && StockQuantity < MinStockThreshold;

    public string UnitLabel => UnitType switch
    {
        UnitType.Gramm => "g",
        _ => UnitType.ToString().ToLower()
    };
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal SellPrice { get; set; }
    public UnitType UnitType { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal? BuyPrice { get; set; }
    public string? Barcode { get; set; }
    public decimal MinStockThreshold { get; set; } = 0;
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal SellPrice { get; set; }
    public UnitType UnitType { get; set; }
    public decimal MinStockThreshold { get; set; } = 0;
}

public class AddStockDto
{
    public decimal Quantity { get; set; }
    public decimal BuyPrice { get; set; }
    public int UserId { get; set; }
}

public class ProductSuggestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal SellPrice { get; set; }
}

public class ProductBatchDto
{
    public int Id { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal OriginalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public DateTime ReceivedAt { get; set; }

    public decimal UsedQuantity => OriginalQuantity - RemainingQuantity;
    public double UsagePercent => OriginalQuantity > 0
        ? (double)(UsedQuantity / OriginalQuantity * 100)
        : 0;
}
