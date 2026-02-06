namespace Application.Exceptions;

public class InsufficientStockException : Exception
{
    public string ProductName { get; }
    public decimal Available { get; }
    public decimal Requested { get; }

    public InsufficientStockException(string productName, decimal available, decimal requested)
        : base($"'{productName}' mahsulotida yetarli zaxira yo'q. Mavjud: {available}, So'ralgan: {requested}")
    {
        ProductName = productName;
        Available = available;
        Requested = requested;
    }
}
