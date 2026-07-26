using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password, string? ipAddress = null);
    Task<AuthResult> RegisterAsync(RegisterRequest request, string? ipAddress = null);
    Task<AuthResult> RefreshTokenAsync(string token, string? ipAddress = null);
    Task RevokeTokenAsync(string token, string? ipAddress = null);
    Task<UserDto?> GetUserAsync(Guid userId);
    Task<List<UserDto>> GetUsersAsync(int pageNumber, int pageSize);
    Task<UserDto?> CreateUserAsync(CreateUserRequest request);
    Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> AssignRoleAsync(Guid userId, Guid roleId);
    Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);
    Task<List<RoleDto>> GetRolesAsync();
    Task<List<PermissionDto>> GetPermissionsAsync();
}
