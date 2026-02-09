using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace Application.Services.Impl;

public class BarcodeService : IBarcodeService
{
    private readonly DatabaseContext _context;
    private readonly Random _random;
    private const int MaxGenerationAttempts = 10;

    public BarcodeService(DatabaseContext context)
    {
        _context = context;
        _random = new Random();
    }

    public async Task<string> GenerateBarcodeAsync()
    {
        int attempts = 0;

        while (attempts < MaxGenerationAttempts)
        {
            // Generate Code128 barcode (timestamp + 4 random digits)
            string barcode = GenerateCode128();

            // Check if unique in database
            bool isUnique = await IsBarcodeUniqueAsync(barcode);

            if (isUnique)
            {
                return barcode;
            }

            attempts++;
        }

        throw new InvalidOperationException($"Failed to generate unique barcode after {MaxGenerationAttempts} attempts");
    }

    public async Task<byte[]> GenerateBarcodeImageAsync(string barcode, string productName, decimal price)
    {
        // Validate barcode format
        if (!await ValidateBarcodeAsync(barcode))
        {
            throw new ArgumentException("Invalid barcode format", nameof(barcode));
        }

        const int barcodeMargin = 5;
        const int barcodeWidth = 300;
        const int barcodeHeight = 80;
        const int finalWidth = 300;
        const int finalHeight = 150;

        // Step 1: Generate barcode using ZXing
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = barcodeWidth,
                Height = barcodeHeight,
                Margin = barcodeMargin,
                PureBarcode = true
            }
        };

        var pixelData = writer.Write(barcode);

        // Step 2: Convert ZXing pixel data directly to SkiaSharp bitmap
        var barcodeInfo = new SKImageInfo(pixelData.Width, pixelData.Height, SKColorType.Bgra8888);
        using var skBitmap = new SKBitmap(barcodeInfo);

        // Copy pixel data to SkiaSharp bitmap
        var pixels = skBitmap.GetPixels();
        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, pixels, pixelData.Pixels.Length);

        // Step 3: Create final image with SkiaSharp
        using var surface = SKSurface.Create(new SKImageInfo(finalWidth, finalHeight));
        var canvas = surface.Canvas;

        // Fill white background
        canvas.Clear(SKColors.White);

        // Draw product name at top
        using var productNameFont = new SKFont
        {
            Size = 14,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };
        using var productNamePaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        float centerX = finalWidth / 2f;
        canvas.DrawText(productName, centerX, 18, SKTextAlign.Center, productNameFont, productNamePaint);

        // Draw barcode in the middle
        canvas.DrawBitmap(skBitmap, 0, 22);

        // Draw price at bottom
        string priceText = $"{price:N0} so'm";
        using var priceFont = new SKFont
        {
            Size = 16,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };
        using var pricePaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        canvas.DrawText(priceText, centerX, 120, SKTextAlign.Center, priceFont, pricePaint);

        // Convert to PNG byte array
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    public Task<bool> ValidateBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return Task.FromResult(false);
        }

        // Code128 validation: ASCII characters, length between 1-80
        if (barcode.Length >= 1 && barcode.Length <= 80)
        {
            // Check if all characters are valid ASCII
            bool isValidAscii = barcode.All(c => c >= 32 && c <= 126);
            return Task.FromResult(isValidAscii);
        }

        // EAN-13 validation: exactly 13 digits
        if (barcode.Length == 13 && barcode.All(char.IsDigit))
        {
            return Task.FromResult(ValidateEan13Checksum(barcode));
        }

        return Task.FromResult(false);
    }

    public async Task<bool> IsBarcodeUniqueAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return false;
        }

        // Check if barcode exists in database (case-insensitive)
        bool exists = await _context.Products
            .AnyAsync(p => p.barcode.ToLower() == barcode.ToLower());

        return !exists;
    }

    // Private helper methods

    private string GenerateCode128()
    {
        // Generate using timestamp + random 4 digits
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 100000000; // Last 8 digits
        int randomDigits = _random.Next(1000, 9999); // 4 random digits

        return $"{timestamp}{randomDigits}";
    }

    private bool ValidateEan13Checksum(string ean13)
    {
        if (ean13.Length != 13 || !ean13.All(char.IsDigit))
        {
            return false;
        }

        // Calculate checksum for EAN-13
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = ean13[i] - '0';
            // Multiply odd positions by 1, even positions by 3
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checksum = (10 - (sum % 10)) % 10;
        int providedChecksum = ean13[12] - '0';

        return checksum == providedChecksum;
    }
}
