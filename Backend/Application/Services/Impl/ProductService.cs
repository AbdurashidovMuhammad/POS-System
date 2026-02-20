using Application.DTOs.CategoryDTOs;
using Application.DTOs.Common;
using Application.DTOs.ProductDTOs;
using Application.Exceptions;
using Core.Entities;
using Core.Enums;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

public class ProductService : IProductService
{
    private readonly DatabaseContext _context;
    private readonly IBarcodeService _barcodeService;
    private readonly IAuditLogService _auditLogService;

    public ProductService(DatabaseContext context, IBarcodeService barcodeService, IAuditLogService auditLogService)
    {
        _context = context;
        _barcodeService = barcodeService;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<ProductDto>> GetAllProductsAsync(PaginationParams paginationParams)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name);

        var totalCount = await query.CountAsync();

        var products = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = products.Select(MapToDto).ToList(),
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }

        return MapToDto(product);
    }

    public async Task<IEnumerable<ProductSuggestDto>> SearchProductsByNameAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<ProductSuggestDto>();
        }

        var pattern = query.Trim() + "%";

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && EF.Functions.ILike(p.Name, pattern))
            .OrderBy(p => p.Name)
            .Take(10)
            .Select(p => new ProductSuggestDto
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.barcode,
                SellPrice = p.SellPrice
            })
            .ToListAsync();

        return products;
    }

    public async Task<PagedResult<ProductDto>> SearchProductsFullAsync(string query, PaginationParams paginationParams)
    {
        var baseQuery = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = query.Trim() + "%";
            baseQuery = baseQuery.Where(p =>
                EF.Functions.ILike(p.Name, pattern) ||
                EF.Functions.ILike(p.barcode, pattern));
        }

        var totalCount = await baseQuery.CountAsync();

        var products = await baseQuery
            .OrderBy(p => p.Name)
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = products.Select(MapToDto).ToList(),
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDto> GetProductByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            throw new ArgumentException("Barcode cannot be empty", nameof(barcode));
        }

        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.barcode.ToLower() == barcode.ToLower());

        if (product == null)
        {
            throw new NotFoundException($"Product with barcode '{barcode}' not found");
        }

        return MapToDto(product);
    }

    public async Task<int> CreateProductAsync(CreateProductDto dto, int userId)
    {
        // Validate category exists
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);

        if (!categoryExists)
        {
            throw new NotFoundException($"Category with ID {dto.CategoryId} not found");
        }

        // Check name uniqueness
        var nameExists = await _context.Products
            .AnyAsync(p => p.Name.ToLower() == dto.Name.ToLower());

        if (nameExists)
        {
            throw new DuplicateException($"Product with name '{dto.Name}' already exists");
        }

        // Validate unit type
        if (!Enum.IsDefined(typeof(Unit_Type), dto.UnitType))
        {
            throw new ArgumentException("Invalid unit type", nameof(dto.UnitType));
        }

        // If initial stock provided, buy price is required
        if (dto.StockQuantity > 0 && (dto.BuyPrice == null || dto.BuyPrice <= 0))
        {
            throw new ArgumentException("Boshlang'ich zaxira uchun kelish narxi kiritilishi shart");
        }

        // Use provided barcode or generate a new one
        string barcode;
        if (!string.IsNullOrWhiteSpace(dto.Barcode))
        {
            var isValid = await _barcodeService.ValidateBarcodeAsync(dto.Barcode.Trim());
            if (!isValid)
                throw new ArgumentException("Barcode formati noto'g'ri");

            var isUnique = await _barcodeService.IsBarcodeUniqueAsync(dto.Barcode.Trim());
            if (!isUnique)
                throw new DuplicateException($"'{dto.Barcode.Trim()}' barcodi allaqachon mavjud");

            barcode = dto.Barcode.Trim();
        }
        else
        {
            barcode = await _barcodeService.GenerateBarcodeAsync();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var product = new Product
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                SellPrice = dto.SellPrice,
                Unit_Type = dto.UnitType,
                StockQuantity = dto.StockQuantity,
                MinStockThreshold = dto.MinStockThreshold,
                barcode = barcode,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Create initial batch if stock provided
            if (dto.StockQuantity > 0 && dto.BuyPrice.HasValue)
            {
                var batch = new ProductBatch
                {
                    ProductId = product.Id,
                    BuyPrice = dto.BuyPrice.Value,
                    OriginalQuantity = dto.StockQuantity,
                    RemainingQuantity = dto.StockQuantity,
                    ReceivedAt = DateTime.Now
                };
                _context.ProductBatches.Add(batch);

                var stockMovement = new StockMovement
                {
                    ProductId = product.Id,
                    MovementType = Movement_Type.StockIn,
                    Quantity = dto.StockQuantity,
                    UnitCost = dto.BuyPrice.Value,
                    UserId = userId,
                    MovementDate = DateTime.Now
                };
                _context.StockMovements.Add(stockMovement);

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            try { await _auditLogService.LogAsync(userId, Action_Type.ProductCreate, "Product", product.Id, $"Mahsulot yaratdi: {dto.Name}"); }
            catch { }

            return product.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto, int userId)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }

        // Validate category exists
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);

        if (!categoryExists)
        {
            throw new NotFoundException($"Category with ID {dto.CategoryId} not found");
        }

        // Check name uniqueness (excluding current product)
        if (!string.Equals(product.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
        {
            var nameExists = await _context.Products
                .AnyAsync(p => p.Name.ToLower() == dto.Name.ToLower() && p.Id != id);

            if (nameExists)
            {
                throw new DuplicateException($"Product with name '{dto.Name}' already exists");
            }
        }

        // Validate unit type
        if (!Enum.IsDefined(typeof(Unit_Type), dto.UnitType))
        {
            throw new ArgumentException("Invalid unit type", nameof(dto.UnitType));
        }

        product.Name = dto.Name;
        product.CategoryId = dto.CategoryId;
        product.SellPrice = dto.SellPrice;
        product.Unit_Type = dto.UnitType;
        product.MinStockThreshold = dto.MinStockThreshold;

        await _context.SaveChangesAsync();

        try { await _auditLogService.LogAsync(userId, Action_Type.ProductUpdate, "Product", id, $"Mahsulotni yangiladi: {dto.Name}"); }
        catch { }

        if (product.CategoryId != dto.CategoryId)
        {
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        }

        return MapToDto(product);
    }

    public async Task<ProductDto> AddStockAsync(int id, AddStockDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new NotFoundException($"Product with ID {id} not found");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
            {
                throw new NotFoundException($"User with ID {dto.UserId} not found");
            }

            // FIFO batch logic: merge if same buy price exists, else create new batch
            var existingBatch = await _context.ProductBatches
                .Where(b => b.ProductId == id
                         && b.BuyPrice == dto.BuyPrice
                         && b.RemainingQuantity > 0)
                .FirstOrDefaultAsync();

            if (existingBatch != null)
            {
                existingBatch.OriginalQuantity += dto.Quantity;
                existingBatch.RemainingQuantity += dto.Quantity;
            }
            else
            {
                _context.ProductBatches.Add(new ProductBatch
                {
                    ProductId = id,
                    BuyPrice = dto.BuyPrice,
                    OriginalQuantity = dto.Quantity,
                    RemainingQuantity = dto.Quantity,
                    ReceivedAt = DateTime.Now
                });
            }

            product.StockQuantity += dto.Quantity;

            _context.StockMovements.Add(new StockMovement
            {
                ProductId = id,
                MovementType = Movement_Type.StockIn,
                Quantity = dto.Quantity,
                UnitCost = dto.BuyPrice,
                UserId = dto.UserId,
                MovementDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            try { await _auditLogService.LogAsync(dto.UserId, Action_Type.StockIn, "Product", id, $"Zaxiraga qo'shdi: {product.Name} +{dto.Quantity} ({dto.BuyPrice:N0} so'm)"); }
            catch { }

            return MapToDto(product);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeactivateProductAsync(int id, int userId)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }

        product.IsActive = false;
        await _context.SaveChangesAsync();

        try { await _auditLogService.LogAsync(userId, Action_Type.ProductDeactivate, "Product", id, $"Mahsulotni o'chirdi: {product.Name}"); }
        catch { }
    }

    public async Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }

    public async Task<List<ProductBatchDto>> GetBatchesAsync(int productId)
    {
        var batches = await _context.ProductBatches
            .AsNoTracking()
            .Where(b => b.ProductId == productId)
            .OrderBy(b => b.ReceivedAt)
            .Select(b => new ProductBatchDto
            {
                Id = b.Id,
                BuyPrice = b.BuyPrice,
                OriginalQuantity = b.OriginalQuantity,
                RemainingQuantity = b.RemainingQuantity,
                ReceivedAt = b.ReceivedAt
            })
            .ToListAsync();

        return batches;
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            Category = product.Category != null ? new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                IsActive = product.Category.IsActive
            } : null,
            SellPrice = product.SellPrice,
            UnitType = product.Unit_Type,
            StockQuantity = product.StockQuantity,
            MinStockThreshold = product.MinStockThreshold,
            Barcode = product.barcode,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }
}
