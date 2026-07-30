namespace LpgErp.Application.Features.Roles.DTOs;

/// <summary>
/// A role as seen by the Roles & Permissions admin screen — richer than Auth's RoleDto (which only
/// serves the role picker on the Users screen), because managing a role needs its permission IDs
/// (to drive the checkbox grid) and how many users hold it (so deleting one can be guarded).
/// </summary>
public class RoleSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<string> Permissions { get; set; } = [];
    public List<Guid> PermissionIds { get; set; } = [];
    public int UserCount { get; set; }
}

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> PermissionIds { get; set; } = [];
}

public class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> PermissionIds { get; set; } = [];
}
