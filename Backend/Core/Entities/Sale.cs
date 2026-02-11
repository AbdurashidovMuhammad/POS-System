using Core.Enums;

namespace Core.Entities;

public class Sale
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public Payment_Type PaymentType { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.Now;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
