using Application.DTOs.CategoryDTOs;
using Application.DTOs.ProductDTOs;
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
    private readonly IAuditLogService _auditLogService;

    public SaleService(DatabaseContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<SaleDto> CreateSaleAsync(CreateSaleDto dto, int userId)
    {
        // Validation: items list is not empty
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new ArgumentException("Savat bo'sh bo'lishi mumkin emas");
        }

        // Validate payment type
        if (!Enum.IsDefined(typeof(Payment_Type), dto.PaymentType))
        {
            throw new ArgumentException("To'lov usuli noto'g'ri");
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

        // Fetch all batches for required products ordered by ReceivedAt (FIFO)
        var batches = await _context.ProductBatches
            .Where(b => productIds.Contains(b.ProductId) && b.RemainingQuantity > 0)
            .OrderBy(b => b.ReceivedAt)
            .ToListAsync();

        var batchesByProduct = batches.GroupBy(b => b.ProductId)
            .ToDictionary(g => g.Key, g => g.OrderBy(b => b.ReceivedAt).ToList());

        // Start transaction
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Calculate total amount (server-side for security)
            decimal totalAmount = 0;
            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];
                totalAmount += product.Unit_Type == Unit_Type.Gramm
                    ? product.SellPrice * item.Quantity / 1000m
                    : product.SellPrice * item.Quantity;
            }

            // Create Sale
            var sale = new Sale
            {
                UserId = userId,
                TotalAmount = totalAmount,
                PaymentType = (Payment_Type)dto.PaymentType,
                SaleDate = DateTime.Now
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Create SaleItems and update stock
            var saleItems = new List<SaleItem>();

            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];

                // FIFO batch consumption — calculate weighted average buy price
                decimal buyPriceAtSale = 0;
                if (batchesByProduct.TryGetValue(item.ProductId, out var productBatches))
                {
                    decimal needed = item.Quantity;
                    decimal totalCost = 0;

                    foreach (var batch in productBatches)
                    {
                        if (needed <= 0) break;
                        var take = Math.Min(batch.RemainingQuantity, needed);
                        totalCost += take * batch.BuyPrice;
                        batch.RemainingQuantity -= take;
                        needed -= take;
                    }

                    buyPriceAtSale = item.Quantity > 0 ? totalCost / item.Quantity : 0;
                }

                var saleItem = new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.SellPrice,
                    BuyPriceAtSale = buyPriceAtSale
                };
                saleItems.Add(saleItem);
                _context.SaleItems.Add(saleItem);

                // Decrease product stock
                product.StockQuantity -= item.Quantity;

                // Create StockMovement (StockOut)
                _context.StockMovements.Add(new StockMovement
                {
                    ProductId = item.ProductId,
                    MovementType = Movement_Type.StockOut,
                    Quantity = item.Quantity,
                    UserId = userId,
                    MovementDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Audit log
            try
            {
                var itemDescriptions = dto.Items.Select(i => $"{products[i.ProductId].Name} x{i.Quantity}");
                var description = $"Sotdi: {string.Join(", ", itemDescriptions)} = {totalAmount:N0} sum";
                await _auditLogService.LogAsync(userId, Action_Type.Sale, "Sale", sale.Id, description);
            }
            catch { /* audit log failure should not break sale */ }

            // Load user for response
            var user = await _context.Users.FindAsync(userId);

            // Map to DTO
            return new SaleDto
            {
                Id = sale.Id,
                UserId = sale.UserId,
                UserFullName = user?.Username ?? "Noma'lum",
                TotalAmount = sale.TotalAmount,
                PaymentType = (int)sale.PaymentType,
                PaymentTypeName = sale.PaymentType.ToString(),
                SaleDate = sale.SaleDate,
                Items = saleItems.Select(si => new SaleItemDto
                {
                    Id = si.Id,
                    ProductId = si.ProductId,
                    ProductName = products[si.ProductId].Name,
                    ProductBarcode = products[si.ProductId].barcode,
                    Quantity = si.Quantity,
                    UnitPrice = si.UnitPrice,
                    BuyPriceAtSale = si.BuyPriceAtSale,
                    UnitType = (int)products[si.ProductId].Unit_Type
                }).ToList()
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SaleDto?> GetSaleByIdAsync(int id)
    {
        var sale = await _context.Sales
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null) return null;

        return new SaleDto
        {
            Id = sale.Id,
            UserId = sale.UserId,
            UserFullName = sale.User.Username,
            TotalAmount = sale.TotalAmount,
            PaymentType = (int)sale.PaymentType,
            PaymentTypeName = sale.PaymentType.ToString(),
            SaleDate = sale.SaleDate,
            Items = sale.SaleItems.Select(si => new SaleItemDto
            {
                Id = si.Id,
                ProductId = si.ProductId,
                ProductName = si.ProductName,
                ProductBarcode = si.Product.barcode,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                BuyPriceAtSale = si.BuyPriceAtSale,
                UnitType = (int)si.Product.Unit_Type
            }).ToList()
        };
    }

    public async Task<List<ProductDto>> GetTopSellingProductsAsync(int count = 5)
    {
        var tomorrow = DateTime.Today.AddDays(1);
        var twoDaysAgo = DateTime.Today.AddDays(-1);

        var topProductIds = await _context.SaleItems
            .AsNoTracking()
            .Where(si => si.Sale.SaleDate >= twoDaysAgo && si.Sale.SaleDate < tomorrow
                      && si.Product.IsActive && si.Product.StockQuantity > 0)
            .GroupBy(si => si.ProductId)
            .OrderByDescending(g => g.Sum(si => si.Quantity))
            .Take(count)
            .Select(g => g.Key)
            .ToListAsync();

        if (topProductIds.Count == 0)
            return [];

        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => topProductIds.Contains(p.Id))
            .ToListAsync();

        var ordered = topProductIds
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .Select(p => new ProductDto
            {
                Id = p!.Id,
                Name = p.Name,
                CategoryId = p.CategoryId,
                Category = p.Category != null ? new CategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    IsActive = p.Category.IsActive
                } : null,
                SellPrice = p.SellPrice,
                UnitType = p.Unit_Type,
                StockQuantity = p.StockQuantity,
                Barcode = p.barcode,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToList();

        return ordered;
    }
}
