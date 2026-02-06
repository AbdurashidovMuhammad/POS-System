namespace Application.DTOs.SaleDTOs;

public class SaleDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
}
