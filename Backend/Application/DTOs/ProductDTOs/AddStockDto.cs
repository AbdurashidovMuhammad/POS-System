using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProductDTOs;

public class AddStockDto
{
    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }
}
