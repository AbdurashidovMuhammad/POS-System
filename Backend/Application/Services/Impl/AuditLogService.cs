using Application.DTOs.AuditLogDTOs;
using Application.DTOs.Common;
using Core.Entities;
using Core.Enums;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

internal class AuditLogService : IAuditLogService
{
    private readonly DatabaseContext _context;

    public AuditLogService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int userId, Action_Type actionType, string entityType, int entityId, string description)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            Timestamp = DateTime.Now
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter)
    {
        var query = _context.AuditLogs
            .Include(a => a.User)
            .AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);

        if (filter.ActionType.HasValue)
            query = query.Where(a => (int)a.ActionType == filter.ActionType.Value);

        if (filter.From.HasValue)
            query = query.Where(a => a.Timestamp >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(a => a.Timestamp < filter.To.Value.Date.AddDays(1));

        query = query.OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Username = a.User.Username,
                ActionType = (int)a.ActionType,
                ActionTypeName = a.ActionType.ToString(),
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Description = a.Description,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }
}
