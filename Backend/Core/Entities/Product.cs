using Core.Enums;

namespace Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int CategoryId { get; set; } 
    public decimal UnitPrice { get; set; }
    public Unit_Type Unit_Type { get; set; }
    public decimal StockQuantity { get; set; } = 0;
    public string barcode { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation property
    public Category Category { get; set; } = null!;
}
