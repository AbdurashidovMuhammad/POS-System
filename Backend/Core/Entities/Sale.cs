namespace Core.Entities;

public class Sale
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User User { get; set; } = null!;
}
