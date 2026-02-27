using Application.DTOs.PermissionDTOs;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Impl;

internal class PermissionService : IPermissionService
{
    private readonly DatabaseContext _context;

    private static readonly Dictionary<string, string> SectionDisplayNames = new()
    {
        ["Products"] = "Mahsulotlar",
        ["Categories"] = "Kategoriyalar",
        ["Sales"] = "Sotuvlar",
    };

    public PermissionService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionGroupDto>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Section)
            .ThenBy(p => p.Id)
            .ToListAsync();

        return permissions
            .GroupBy(p => p.Section)
            .Select(g => new PermissionGroupDto
            {
                Section = g.Key,
                SectionDisplayName = SectionDisplayNames.TryGetValue(g.Key, out var name) ? name : g.Key,
                Permissions = g.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Section = p.Section,
                    Action = p.Action,
                    DisplayName = p.DisplayName
                }).ToList()
            })
            .ToList();
    }

    public async Task<UserPermissionsDto> GetUserPermissionsAsync(int userId)
    {
        var permissionIds = await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => up.PermissionId)
            .ToListAsync();

        return new UserPermissionsDto
        {
            UserId = userId,
            PermissionIds = permissionIds
        };
    }

    public async Task UpdateUserPermissionsAsync(int userId, List<int> permissionIds)
    {
        var existing = await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .ToListAsync();

        _context.UserPermissions.RemoveRange(existing);

        if (permissionIds.Count > 0)
        {
            var validIds = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var newPermissions = validIds.Select(pid => new Core.Entities.UserPermission
            {
                UserId = userId,
                PermissionId = pid
            });

            await _context.UserPermissions.AddRangeAsync(newPermissions);
        }

        await _context.SaveChangesAsync();
    }
}
