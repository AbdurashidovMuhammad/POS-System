using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.SaleDTOs;

public class CreateSaleItemDto
{
    [Required(ErrorMessage = "Mahsulot ID kiritilishi shart")]
    [Range(1, int.MaxValue, ErrorMessage = "Mahsulot ID noto'g'ri")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Miqdor kiritilishi shart")]
    [Range(0.001, double.MaxValue, ErrorMessage = "Miqdor 0 dan katta bo'lishi kerak")]
    public decimal Quantity { get; set; }
}
