namespace Application.DTOs.SaleDTOs;

public class SaleItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string ProductBarcode { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal BuyPriceAtSale { get; set; }
    public int UnitType { get; set; }
    public decimal Subtotal => UnitType == (int)Core.Enums.Unit_Type.Gramm
        ? Quantity * UnitPrice / 1000m
        : Quantity * UnitPrice;
    public decimal BuyCost => UnitType == (int)Core.Enums.Unit_Type.Gramm
        ? Quantity * BuyPriceAtSale / 1000m
        : Quantity * BuyPriceAtSale;
    public decimal Profit => Subtotal - BuyCost;
}
