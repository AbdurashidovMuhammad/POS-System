namespace Application.DTOs.SaleDTOs;

public class SaleDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public int PaymentType { get; set; }
    public string PaymentTypeName { get; set; } = null!;
    public DateTime SaleDate { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
}
