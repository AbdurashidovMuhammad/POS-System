using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProductDTOs;

public class UpdateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Sotish narxi kiritilishi shart")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Sotish narxi 0 dan katta bo'lishi kerak")]
    public decimal SellPrice { get; set; }

    [Required(ErrorMessage = "Unit type is required")]
    public Unit_Type UnitType { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Min. zaxira chegarasi manfiy bo'lishi mumkin emas")]
    public decimal MinStockThreshold { get; set; } = 0;
}
