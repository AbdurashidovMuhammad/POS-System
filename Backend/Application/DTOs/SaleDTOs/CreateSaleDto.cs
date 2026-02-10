using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.SaleDTOs;

public class CreateSaleDto
{
    [Required(ErrorMessage = "Savat bo'sh bo'lishi mumkin emas")]
    [MinLength(1, ErrorMessage = "Kamida bitta mahsulot bo'lishi kerak")]
    public List<CreateSaleItemDto> Items { get; set; } = new();

    [Required(ErrorMessage = "To'lov usuli tanlanishi shart")]
    [Range(1, 7, ErrorMessage = "To'lov usuli noto'g'ri")]
    public int PaymentType { get; set; }
}
