using Application.DTOs.Common;
using Application.DTOs.ReportDTOs;
using Application.Services;
using ClosedXML.Excel;
using Core.Enums;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

public class ReportService : IReportService
{
    private readonly DatabaseContext _context;

    public ReportService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, PaginationParams pagination, int? userId = null)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1);

        var query = _context.SaleItems
            .AsNoTracking()
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .Where(si => si.Sale.SaleDate >= fromDate && si.Sale.SaleDate < toDate);

        if (userId.HasValue)
            query = query.Where(si => si.Sale.UserId == userId.Value);

        var totalCount = await query.CountAsync();

        var orderCount = await query.Select(si => si.Sale.Id).Distinct().CountAsync();

        var totalAmount = await query.SumAsync(si => si.Quantity * si.UnitPrice);
        var totalProfit = await query.SumAsync(si => si.Quantity * si.UnitPrice - si.Quantity * si.BuyPriceAtSale);

        var items = await query
            .OrderByDescending(si => si.Sale.SaleDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(si => new SalesReportItemDto
            {
                SaleId = si.Sale.Id,
                Date = si.Sale.SaleDate,
                ProductName = si.ProductName,
                Quantity = si.Quantity,
                UnitTypeName = si.Product.Unit_Type.ToString(),
                UnitPrice = si.UnitPrice,
                BuyPriceAtSale = si.BuyPriceAtSale,
                TotalPrice = si.Quantity * si.UnitPrice,
                Profit = si.Quantity * si.UnitPrice - si.Quantity * si.BuyPriceAtSale,
                PaymentTypeName = si.Sale.PaymentType.ToString(),
                Username = si.Sale.User.Username
            })
            .ToListAsync();

        return new SalesReportDto
        {
            DateFrom = from.Date,
            DateTo = to.Date,
            Items = items,
            TotalAmount = totalAmount,
            TotalProfit = totalProfit,
            OrderCount = orderCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrdersReportDto> GetOrdersReportAsync(DateTime from, DateTime to, PaginationParams pagination, int? userId = null)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1);

        var query = _context.Sales
            .AsNoTracking()
            .Where(s => s.SaleDate >= fromDate && s.SaleDate < toDate);

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);

        var totalCount = await query.CountAsync();

        var totalAmount = await _context.SaleItems
            .AsNoTracking()
            .Where(si => si.Sale.SaleDate >= fromDate && si.Sale.SaleDate < toDate
                         && (!userId.HasValue || si.Sale.UserId == userId.Value))
            .SumAsync(si => si.Quantity * si.UnitPrice);

        var orders = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(s => new OrderReportItemDto
            {
                SaleId = s.Id,
                Date = s.SaleDate,
                TotalAmount = s.SaleItems.Sum(si => si.Quantity * si.UnitPrice),
                PaymentTypeName = s.PaymentType.ToString(),
                Username = s.User.Username,
                ItemCount = s.SaleItems.Count,
                Items = s.SaleItems.Select(si => new OrderReportProductDto
                {
                    ProductName = si.ProductName,
                    Quantity = si.Quantity,
                    UnitTypeName = si.Product.Unit_Type.ToString(),
                    UnitPrice = si.UnitPrice,
                    TotalPrice = si.Quantity * si.UnitPrice
                }).ToList()
            })
            .ToListAsync();

        return new OrdersReportDto
        {
            DateFrom = from.Date,
            DateTo = to.Date,
            Items = orders,
            TotalAmount = totalAmount,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<StockInReportDto> GetStockInReportAsync(DateTime from, DateTime to, PaginationParams pagination, int? userId = null)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1);

        var query = _context.StockMovements
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Where(sm => sm.MovementType == Movement_Type.StockIn
                         && sm.MovementDate >= fromDate
                         && sm.MovementDate < toDate);

        if (userId.HasValue)
            query = query.Where(sm => sm.UserId == userId.Value);

        var totalCount = await query.CountAsync();

        var totalQuantity = await query.SumAsync(sm => sm.Quantity);
        var totalAmount = await query.SumAsync(sm => sm.Quantity * (sm.UnitCost ?? 0));

        var quantityByUnit = await query
            .GroupBy(sm => sm.Product.Unit_Type)
            .Select(g => new UnitQuantitySummaryDto
            {
                UnitTypeName = g.Key.ToString(),
                TotalQuantity = g.Sum(sm => sm.Quantity)
            })
            .ToListAsync();

        var items = await query
            .OrderByDescending(sm => sm.MovementDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(sm => new StockInReportItemDto
            {
                Date = sm.MovementDate,
                ProductName = sm.Product.Name,
                Quantity = sm.Quantity,
                UnitTypeName = sm.Product.Unit_Type.ToString(),
                Username = sm.User.Username
            })
            .ToListAsync();

        return new StockInReportDto
        {
            DateFrom = from.Date,
            DateTo = to.Date,
            Items = items,
            TotalQuantity = totalQuantity,
            TotalAmount = totalAmount,
            QuantityByUnit = quantityByUnit,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<byte[]> ExportSalesReportAsync(DateTime from, DateTime to, int? userId = null)
    {
        var report = await GetAllSalesItemsAsync(from, to, userId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sotilgan mahsulotlar");

        // Title
        worksheet.Cell(1, 1).Value = $"Sotilgan mahsulotlar hisoboti ({from:dd.MM.yyyy} - {to:dd.MM.yyyy})";
        worksheet.Range(1, 1, 1, 9).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Headers
        var headerRow = 3;
        var headers = new[] { "Sana", "Mahsulot", "Miqdor", "Sotish narxi", "Kelish narxi", "Jami", "Foyda", "To'lov usuli", "Foydalanuvchi" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4CAF50");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Data rows
        var row = headerRow + 1;
        foreach (var item in report.Items)
        {
            worksheet.Cell(row, 1).Value = item.Date.ToString("dd.MM.yyyy HH:mm");
            worksheet.Cell(row, 2).Value = item.ProductName;
            worksheet.Cell(row, 3).Value = $"{item.Quantity} {item.UnitTypeName}";
            worksheet.Cell(row, 4).Value = item.UnitPrice;
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0 \"so'm\"";
            worksheet.Cell(row, 5).Value = item.BuyPriceAtSale;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0 \"so'm\"";
            worksheet.Cell(row, 6).Value = item.TotalPrice;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0 \"so'm\"";
            worksheet.Cell(row, 7).Value = item.Profit;
            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0 \"so'm\"";
            worksheet.Cell(row, 8).Value = item.PaymentTypeName;
            worksheet.Cell(row, 9).Value = item.Username;

            if (row % 2 == 0)
            {
                worksheet.Range(row, 1, row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#f5f5f5");
            }

            for (int col = 1; col <= 9; col++)
            {
                worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.OutsideBorderColor = XLColor.FromHtml("#e0e0e0");
            }

            row++;
        }

        // Footer - total
        worksheet.Cell(row, 1).Value = "JAMI:";
        worksheet.Range(row, 1, row, 5).Merge();
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Cell(row, 6).Value = report.TotalAmount;
        worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0 \"so'm\"";
        worksheet.Cell(row, 6).Style.Font.Bold = true;
        worksheet.Cell(row, 7).Value = report.TotalProfit;
        worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0 \"so'm\"";
        worksheet.Cell(row, 7).Style.Font.Bold = true;
        worksheet.Range(row, 1, row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#e8f5e9");
        for (int col = 1; col <= 9; col++)
        {
            worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportOrdersReportAsync(DateTime from, DateTime to, int? userId = null)
    {
        var orders = await GetAllOrdersAsync(from, to, userId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Buyurtmalar");

        // Title
        worksheet.Cell(1, 1).Value = $"Buyurtmalar hisoboti ({from:dd.MM.yyyy} - {to:dd.MM.yyyy})";
        worksheet.Range(1, 1, 1, 6).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Headers
        var headerRow = 3;
        var headers = new[] { "Sana", "Buyurtma #", "Mahsulot", "Miqdor", "Narxi", "To'lov usuli" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FF9800");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Data rows
        var row = headerRow + 1;
        foreach (var order in orders)
        {
            foreach (var item in order.Items)
            {
                worksheet.Cell(row, 1).Value = order.Date.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cell(row, 2).Value = order.SaleId;
                worksheet.Cell(row, 3).Value = item.ProductName;
                worksheet.Cell(row, 4).Value = $"{item.Quantity:0.##} {item.UnitTypeName}";
                worksheet.Cell(row, 5).Value = item.TotalPrice;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0 \"so'm\"";
                worksheet.Cell(row, 6).Value = order.PaymentTypeName;

                if (row % 2 == 0)
                {
                    worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#f5f5f5");
                }

                for (int col = 1; col <= 6; col++)
                {
                    worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.OutsideBorderColor = XLColor.FromHtml("#e0e0e0");
                }

                row++;
            }
        }

        // Footer - total
        var totalAmount = orders.Sum(o => o.TotalAmount);
        worksheet.Cell(row, 1).Value = "JAMI:";
        worksheet.Range(row, 1, row, 4).Merge();
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Cell(row, 5).Value = totalAmount;
        worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0 \"so'm\"";
        worksheet.Cell(row, 5).Style.Font.Bold = true;
        worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#fff3e0");
        for (int col = 1; col <= 6; col++)
        {
            worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportStockInReportAsync(DateTime from, DateTime to, int? userId = null)
    {
        var report = await GetAllStockInItemsAsync(from, to, userId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Kirim mahsulotlar");

        // Title
        worksheet.Cell(1, 1).Value = $"Kirib kelgan mahsulotlar hisoboti ({from:dd.MM.yyyy} - {to:dd.MM.yyyy})";
        worksheet.Range(1, 1, 1, 4).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Headers
        var headerRow = 3;
        var headers = new[] { "Sana", "Mahsulot", "Miqdor", "Foydalanuvchi" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2196F3");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Data rows
        var row = headerRow + 1;
        foreach (var item in report.Items)
        {
            worksheet.Cell(row, 1).Value = item.Date.ToString("dd.MM.yyyy HH:mm");
            worksheet.Cell(row, 2).Value = item.ProductName;
            worksheet.Cell(row, 3).Value = $"{item.Quantity} {item.UnitTypeName}";
            worksheet.Cell(row, 4).Value = item.Username;

            if (row % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#f5f5f5");
            }

            for (int col = 1; col <= 4; col++)
            {
                worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.OutsideBorderColor = XLColor.FromHtml("#e0e0e0");
            }

            row++;
        }

        // Footer - total
        worksheet.Cell(row, 1).Value = "JAMI MIQDOR:";
        worksheet.Range(row, 1, row, 2).Merge();
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Cell(row, 3).Value = report.TotalQuantity;
        worksheet.Cell(row, 3).Style.Font.Bold = true;
        worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#e3f2fd");
        for (int col = 1; col <= 4; col++)
        {
            worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task<SalesReportDto> GetAllSalesItemsAsync(DateTime from, DateTime to, int? userId = null)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1);

        var query = _context.SaleItems
            .AsNoTracking()
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .Where(si => si.Sale.SaleDate >= fromDate && si.Sale.SaleDate < toDate);

        if (userId.HasValue)
            query = query.Where(si => si.Sale.UserId == userId.Value);

        var items = await query
            .OrderByDescending(si => si.Sale.SaleDate)
            .Select(si => new SalesReportItemDto
            {
                SaleId = si.Sale.Id,
                Date = si.Sale.SaleDate,
                ProductName = si.ProductName,
                Quantity = si.Quantity,
                UnitTypeName = si.Product.Unit_Type.ToString(),
                UnitPrice = si.UnitPrice,
                BuyPriceAtSale = si.BuyPriceAtSale,
                TotalPrice = si.Quantity * si.UnitPrice,
                Profit = si.Quantity * si.UnitPrice - si.Quantity * si.BuyPriceAtSale,
                PaymentTypeName = si.Sale.PaymentType.ToString(),
                Username = si.Sale.User.Username
            })
            .ToListAsync();

        return new SalesReportDto
        {
            DateFrom = from.Date,
            DateTo = to.Date,
            Items = items,
            TotalAmount = items.Sum(i => i.TotalPrice),
            TotalProfit = items.Sum(i => i.Profit),
            TotalCount = items.Count,
            Page = 1,
            PageSize = items.Count
        };
    }

    private async Task<List<OrderReportItemDto>> GetAllOrdersAsync(DateTime from, DateTime to, int? userId = null)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1);

        var query = _context.Sales
            .AsNoTracking()
            .Where(s => s.SaleDate >= fromDate && s.SaleDate < toDate);

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);

        return await query
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new OrderReportItemDto
            {
                SaleId = s.Id,
                Date = s.SaleDate,
                TotalAmount = s.SaleItems.Sum(si => si.Quantity * si.UnitPrice),
                PaymentTypeName = s.PaymentType.ToString(),
                Username = s.User.Username,
                ItemCount = s.SaleItems.Count,
                Items = s.SaleItems.Select(si => new OrderReportProductDto
                {
                    ProductName = si.ProductName,
                    Quantity = si.Quantity,
                    UnitTypeName = si.Product.Unit_Type.ToString(),
                    UnitPrice = si.UnitPrice,
                    TotalPrice = si.Quantity * si.UnitPrice
                }).ToList()
            })
            .ToListAsync();
    }

    private async Task<StockInReportDto> GetAllStockInItemsAsync(DateTime from, DateTime to, int? userId = null)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1);

        var query = _context.StockMovements
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Where(sm => sm.MovementType == Movement_Type.StockIn
                         && sm.MovementDate >= fromDate
                         && sm.MovementDate < toDate);

        if (userId.HasValue)
            query = query.Where(sm => sm.UserId == userId.Value);

        var items = await query
            .OrderByDescending(sm => sm.MovementDate)
            .Select(sm => new StockInReportItemDto
            {
                Date = sm.MovementDate,
                ProductName = sm.Product.Name,
                Quantity = sm.Quantity,
                UnitTypeName = sm.Product.Unit_Type.ToString(),
                Username = sm.User.Username
            })
            .ToListAsync();

        var totalAmount = await query.SumAsync(sm => sm.Quantity * (sm.UnitCost ?? 0));

        return new StockInReportDto
        {
            DateFrom = from.Date,
            DateTo = to.Date,
            Items = items,
            TotalQuantity = items.Sum(i => i.Quantity),
            TotalAmount = totalAmount,
            TotalCount = items.Count,
            Page = 1,
            PageSize = items.Count
        };
    }
}
