namespace Application.DTOs.ProductDTOs;

public class ProductSuggestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Barcode { get; set; } = null!;
    public decimal SellPrice { get; set; }
}
