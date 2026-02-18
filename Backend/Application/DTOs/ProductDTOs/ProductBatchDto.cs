namespace Application.DTOs.ProductDTOs;

public class ProductBatchDto
{
    public int Id { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal OriginalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public DateTime ReceivedAt { get; set; }
}
