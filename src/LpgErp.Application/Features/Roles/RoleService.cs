using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Roles.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.Roles;

public interface IRoleService
{
    Task<Result<IReadOnlyList<RoleSummaryDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<RoleSummaryDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<RoleSummaryDto>> CreateAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task<Result<RoleSummaryDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Role *definitions* — which permissions a role grants. Distinct from Auth's user/role-assignment
/// endpoints, which only ever read roles to populate a picker; this is what lets an admin create a
/// role or change what an existing one can do, instead of that being fixed forever at seed time.
/// </summary>
public class RoleService : IRoleService
{
    private readonly IApplicationDbContext _context;

    public RoleService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<RoleSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var roles = await Query().OrderBy(r => r.Name).ToListAsync(ct);
        var dtos = new List<RoleSummaryDto>(roles.Count);
        foreach (var role in roles)
            dtos.Add(await MapToDtoAsync(role, ct));

        return Result<IReadOnlyList<RoleSummaryDto>>.Success(dtos);
    }

    public async Task<Result<RoleSummaryDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var role = await Query().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (role is null) return Result<RoleSummaryDto>.Failure("Role not found.");
        return Result<RoleSummaryDto>.Success(await MapToDtoAsync(role, ct));
    }

    public async Task<Result<RoleSummaryDto>> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        if (await _context.Roles.AnyAsync(r => !r.IsDeleted && r.Name == request.Name, ct))
            return Result<RoleSummaryDto>.Failure($"A role named '{request.Name}' already exists.");

        if (await InvalidPermissionIdsAsync(request.PermissionIds, ct) is string permError)
            return Result<RoleSummaryDto>.Failure(permError);

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
        };
        foreach (var permissionId in request.PermissionIds.Distinct())
            role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);

        return await GetByIdAsync(role.Id, ct);
    }

    public async Task<Result<RoleSummaryDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);
        if (role is null) return Result<RoleSummaryDto>.Failure("Role not found.");

        if (await _context.Roles.AnyAsync(r => !r.IsDeleted && r.Id != id && r.Name == request.Name, ct))
            return Result<RoleSummaryDto>.Failure($"A role named '{request.Name}' already exists.");

        if (await InvalidPermissionIdsAsync(request.PermissionIds, ct) is string permError)
            return Result<RoleSummaryDto>.Failure(permError);

        role.Name = request.Name;
        role.Description = request.Description;
        role.IsActive = request.IsActive;

        // Full replace rather than diffing — a permission grid is edited and saved as one set,
        // not as an incremental add/remove sequence like a user's roles are.
        _context.RolePermissions.RemoveRange(role.RolePermissions);
        foreach (var permissionId in request.PermissionIds.Distinct())
            role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });

        await _context.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);
        if (role is null) return Result.Failure("Role not found.");

        // "Admin" is the only role guaranteed to reach user/role management at all — deleting it
        // would be unrecoverable without direct database access. Rename is still allowed; only
        // deletion is blocked, since renaming doesn't remove the capability from whoever holds it.
        if (role.Name == "Admin")
            return Result.Failure("The Admin role cannot be deleted.");

        var userCount = await ActiveUserCountAsync(id, ct);
        if (userCount > 0)
            return Result.Failure($"This role is assigned to {userCount} user(s) and cannot be deleted. Remove it from them first.");

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    private IQueryable<Role> Query() =>
        _context.Roles
            .Where(r => !r.IsDeleted)
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission);

    private async Task<string?> InvalidPermissionIdsAsync(List<Guid> permissionIds, CancellationToken ct)
    {
        if (permissionIds.Count == 0) return null;

        var validCount = await _context.Permissions.CountAsync(p => permissionIds.Contains(p.Id), ct);
        return validCount == permissionIds.Distinct().Count() ? null : "One or more permissions were not recognized.";
    }

    /// <summary>
    /// Users actually holding this role. Deleting a user only soft-deletes it — the UserRole row
    /// is never cleaned up — so counting every UserRole regardless of the user's own IsDeleted flag
    /// would count departed staff forever, making a role permanently undeletable the moment anyone,
    /// past or present, was ever assigned it.
    /// </summary>
    private Task<int> ActiveUserCountAsync(Guid roleId, CancellationToken ct) =>
        _context.UserRoles.CountAsync(ur => ur.RoleId == roleId && !ur.User.IsDeleted, ct);

    private async Task<RoleSummaryDto> MapToDtoAsync(Role role, CancellationToken ct) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Description = role.Description,
        IsActive = role.IsActive,
        Permissions = role.RolePermissions.Select(rp => rp.Permission.Name).OrderBy(n => n).ToList(),
        PermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList(),
        UserCount = await ActiveUserCountAsync(role.Id, ct),
    };
}
