namespace WPF.Models;

public class CreateSaleItemDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
}

public class CreateSaleDto
{
    public List<CreateSaleItemDto> Items { get; set; } = new();
    public int PaymentType { get; set; }
}

public class SaleItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductBarcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal BuyPriceAtSale { get; set; }
    public int UnitType { get; set; }
    public decimal Subtotal => UnitType == (int)Enums.UnitType.Gramm
        ? Quantity * UnitPrice / 1000m
        : Quantity * UnitPrice;
    public string FormattedQuantity => Quantity.ToString("0.##");
    public string UnitTypeName => UnitType switch
    {
        8 => "g",
        _ => ((Enums.UnitType)UnitType).ToString().ToLower()
    };
}

public class SaleDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int PaymentType { get; set; }
    public string PaymentTypeName { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
}
