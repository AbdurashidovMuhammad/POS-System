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

        var lowerQuery = query.Trim().ToLower();

        var products = await _context.Products
            .Where(p => p.IsActive && p.Name.ToLower().StartsWith(lowerQuery))
            .OrderBy(p => p.Name)
            .Take(10)
            .Select(p => new ProductSuggestDto
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.barcode,
                UnitPrice = p.UnitPrice
            })
            .ToListAsync();

        return products;
    }

    public async Task<PagedResult<ProductDto>> SearchProductsFullAsync(string query, PaginationParams paginationParams)
    {
        var baseQuery = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.Trim().ToLower();
            baseQuery = baseQuery.Where(p =>
                p.Name.ToLower().StartsWith(lowerQuery) ||
                p.barcode.ToLower().StartsWith(lowerQuery));
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

        // Generate unique barcode
        var barcode = await _barcodeService.GenerateBarcodeAsync();

        // Create product
        var product = new Product
        {
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            UnitPrice = dto.UnitPrice,
            Unit_Type = dto.UnitType,
            StockQuantity = dto.StockQuantity,
            barcode = barcode,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        try { await _auditLogService.LogAsync(userId, Action_Type.ProductCreate, "Product", product.Id, $"Mahsulot yaratdi: {dto.Name}"); }
        catch { }

        return product.Id;
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
        if (product.Name.ToLower() != dto.Name.ToLower())
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

        // Update product
        product.Name = dto.Name;
        product.CategoryId = dto.CategoryId;
        product.UnitPrice = dto.UnitPrice;
        product.Unit_Type = dto.UnitType;

        await _context.SaveChangesAsync();

        try { await _auditLogService.LogAsync(userId, Action_Type.ProductUpdate, "Product", id, $"Mahsulotni yangiladi: {dto.Name}"); }
        catch { }

        // Reload category if changed
        if (product.CategoryId != dto.CategoryId)
        {
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        }

        return MapToDto(product);
    }

    public async Task<ProductDto> AddStockAsync(int id, AddStockDto dto)
    {
        // Start transaction
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

            // Validate user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
            {
                throw new NotFoundException($"User with ID {dto.UserId} not found");
            }

            // Add to product stock
            product.StockQuantity += dto.Quantity;

            // Create stock movement record
            var stockMovement = new StockMovement
            {
                ProductId = id,
                MovementType = Movement_Type.StockIn,
                Quantity = dto.Quantity,
                UserId = dto.UserId,
                MovementDate = DateTime.Now
            };

            _context.StockMovements.Add(stockMovement);

            // Save changes
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            try { await _auditLogService.LogAsync(dto.UserId, Action_Type.StockIn, "Product", id, $"Zaxiraga qo'shdi: {product.Name} +{dto.Quantity}"); }
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
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }

    // Private helper method to map Product to ProductDto
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
            UnitPrice = product.UnitPrice,
            UnitType = product.Unit_Type,
            StockQuantity = product.StockQuantity,
            Barcode = product.barcode,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }
}
