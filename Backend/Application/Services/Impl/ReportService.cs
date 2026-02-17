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

        var items = await query
            .OrderByDescending(si => si.Sale.SaleDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(si => new SalesReportItemDto
            {
                Date = si.Sale.SaleDate,
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                UnitTypeName = si.Product.Unit_Type.ToString(),
                UnitPrice = si.UnitPrice,
                TotalPrice = si.Quantity * si.UnitPrice,
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
            OrderCount = orderCount,
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
        worksheet.Range(1, 1, 1, 7).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Headers
        var headerRow = 3;
        var headers = new[] { "Sana", "Mahsulot", "Miqdor", "Narxi", "Jami", "To'lov usuli", "Foydalanuvchi" };
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
            worksheet.Cell(row, 5).Value = item.TotalPrice;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0 \"so'm\"";
            worksheet.Cell(row, 6).Value = item.PaymentTypeName;
            worksheet.Cell(row, 7).Value = item.Username;

            // Alternate row colors
            if (row % 2 == 0)
            {
                worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#f5f5f5");
            }

            for (int col = 1; col <= 7; col++)
            {
                worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.OutsideBorderColor = XLColor.FromHtml("#e0e0e0");
            }

            row++;
        }

        // Footer - total
        worksheet.Cell(row, 1).Value = "JAMI:";
        worksheet.Range(row, 1, row, 4).Merge();
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Cell(row, 5).Value = report.TotalAmount;
        worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0 \"so'm\"";
        worksheet.Cell(row, 5).Style.Font.Bold = true;
        worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#e8f5e9");
        for (int col = 1; col <= 7; col++)
        {
            worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Auto-fit columns
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

        // Auto-fit columns
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
                Date = si.Sale.SaleDate,
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                UnitTypeName = si.Product.Unit_Type.ToString(),
                UnitPrice = si.UnitPrice,
                TotalPrice = si.Quantity * si.UnitPrice,
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
            TotalCount = items.Count,
            Page = 1,
            PageSize = items.Count
        };
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

        return new StockInReportDto
        {
            DateFrom = from.Date,
            DateTo = to.Date,
            Items = items,
            TotalQuantity = items.Sum(i => i.Quantity),
            TotalCount = items.Count,
            Page = 1,
            PageSize = items.Count
        };
    }
}
