using Application.DTOs.SaleDTOs;
using Application.Exceptions;
using Core.Entities;
using Core.Enums;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

public class SaleService : ISaleService
{
    private readonly DatabaseContext _context;

    public SaleService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<SaleDto> CreateSaleAsync(CreateSaleDto dto, int userId)
    {
        // Validation: items list is not empty
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new ArgumentException("Savat bo'sh bo'lishi mumkin emas");
        }

        // Get all product IDs from items
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

        // Fetch all required products in one query
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // Validate all products exist and are active
        foreach (var item in dto.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                throw new NotFoundException($"Mahsulot ID {item.ProductId} topilmadi");
            }

            if (!product.IsActive)
            {
                throw new NotFoundException($"'{product.Name}' mahsuloti faol emas");
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException($"'{product.Name}' mahsuloti uchun miqdor 0 dan katta bo'lishi kerak");
            }

            // Check stock availability
            if (product.StockQuantity < item.Quantity)
            {
                throw new InsufficientStockException(product.Name, product.StockQuantity, item.Quantity);
            }
        }

        // Start transaction
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Calculate total amount (server-side for security)
            decimal totalAmount = 0;
            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];
                totalAmount += product.UnitPrice * item.Quantity;
            }

            // Create Sale
            var sale = new Sale
            {
                UserId = userId,
                TotalAmount = totalAmount,
                SaleDate = DateTime.UtcNow
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Create SaleItems and update stock
            var saleItems = new List<SaleItem>();

            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];

                // Create SaleItem with historical price
                var saleItem = new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.UnitPrice // Historical price snapshot
                };
                saleItems.Add(saleItem);
                _context.SaleItems.Add(saleItem);

                // Decrease product stock
                product.StockQuantity -= item.Quantity;

                // Create StockMovement (StockOut)
                var stockMovement = new StockMovement
                {
                    ProductId = item.ProductId,
                    MovementType = Movement_Type.StockOut,
                    Quantity = item.Quantity,
                    UserId = userId,
                    MovementDate = DateTime.UtcNow
                };
                _context.StockMovements.Add(stockMovement);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Load user for response
            var user = await _context.Users.FindAsync(userId);

            // Map to DTO
            return new SaleDto
            {
                Id = sale.Id,
                UserId = sale.UserId,
                UserFullName = user?.Username ?? "Noma'lum",
                TotalAmount = sale.TotalAmount,
                SaleDate = sale.SaleDate,
                Items = saleItems.Select(si => new SaleItemDto
                {
                    Id = si.Id,
                    ProductId = si.ProductId,
                    ProductName = products[si.ProductId].Name,
                    ProductBarcode = products[si.ProductId].barcode,
                    Quantity = si.Quantity,
                    UnitPrice = si.UnitPrice
                }).ToList()
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
