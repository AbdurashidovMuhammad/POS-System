namespace Application.Services;

public interface IBarcodeService
{
    /// <summary>
    /// Generates a unique barcode string (Code128 or EAN-13 format)
    /// </summary>
    /// <returns>Unique barcode string</returns>
    Task<string> GenerateBarcodeAsync();

    /// <summary>
    /// Generates a barcode image with product name and price overlay
    /// </summary>
    /// <param name="barcode">Barcode string to encode</param>
    /// <param name="productName">Product name to display on top</param>
    /// <param name="price">Price to display at bottom</param>
    /// <returns>PNG image as byte array</returns>
    Task<byte[]> GenerateBarcodeImageAsync(string barcode, string productName, decimal price);

    /// <summary>
    /// Validates if a barcode string has correct format
    /// </summary>
    /// <param name="barcode">Barcode to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    Task<bool> ValidateBarcodeAsync(string barcode);

    /// <summary>
    /// Checks if a barcode is unique in the database
    /// </summary>
    /// <param name="barcode">Barcode to check</param>
    /// <returns>True if unique, false if already exists</returns>
    Task<bool> IsBarcodeUniqueAsync(string barcode);
}
