using Core.Enums;

namespace Core.Entities;

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Movement_Type MovementType { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.Now;
    public int UserId { get; set; }


    // Navigation property
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}
