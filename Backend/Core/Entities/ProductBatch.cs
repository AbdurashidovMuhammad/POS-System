namespace Core.Entities;

public class ProductBatch
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal OriginalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.Now;

    // Navigation property
    public Product Product { get; set; } = null!;
}
