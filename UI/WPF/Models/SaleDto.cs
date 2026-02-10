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
    public decimal Subtotal => Quantity * UnitPrice;
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
