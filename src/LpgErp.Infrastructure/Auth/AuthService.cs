using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Features.Auth;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using LpgErp.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LpgErp.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IApplicationDbContext context,
        IOptions<JwtSettings> jwtSettings,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _currentUserService = currentUserService;
    }

    public async Task<AuthResult> LoginAsync(string username, string password, string? ipAddress = null)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null || !VerifyPassword(password, user.PasswordHash))
            return new AuthResult { IsSuccess = false, Error = "Invalid username or password" };

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GenerateAuthResult(user, ipAddress);
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, string? ipAddress = null)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return new AuthResult { IsSuccess = false, Error = "Username already exists" };

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return new AuthResult { IsSuccess = false, Error = "Email already exists" };

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            IsActive = true,
            LastLoginAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        if (!string.IsNullOrEmpty(request.RoleName))
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName);
            if (role != null)
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, AssignedBy = _currentUserService.UserId });
            }
        }

        await _context.SaveChangesAsync();
        return await GenerateAuthResult(user, ipAddress);
    }

    public async Task<AuthResult> RefreshTokenAsync(string token, string? ipAddress = null)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
            return new AuthResult { IsSuccess = false, Error = "Invalid refresh token" };

        // Token reuse detection: a revoked token was presented — revoke the entire family
        if (!refreshToken.IsActive)
        {
            if (refreshToken.RevokedAt != null)
            {
                await RevokeTokenFamilyAsync(refreshToken.FamilyId, "Token reuse detected");
            }
            return new AuthResult { IsSuccess = false, Error = "Refresh token has been revoked" };
        }

        if (!refreshToken.User.IsActive)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokeReason = "Account deactivated";
            await _context.SaveChangesAsync();
            return new AuthResult { IsSuccess = false, Error = "Account is deactivated" };
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokeReason = "Replaced by refresh";
        return await GenerateAuthResult(refreshToken.User, ipAddress, refreshToken.FamilyId);
    }

    public async Task RevokeTokenAsync(string token, string? ipAddress = null)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.IsActive);

        if (refreshToken != null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokeReason = "Manually revoked";
            await _context.SaveChangesAsync();
        }
    }

    private async Task RevokeTokenFamilyAsync(Guid familyId, string reason)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.FamilyId == familyId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokeReason = reason;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user == null ? null : MapToDto(user);
    }

    public async Task<List<UserDto>> GetUsersAsync(int pageNumber, int pageSize)
    {
        var users = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .OrderBy(u => u.Username)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return null;

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            IsActive = true
        };

        _context.Users.Add(user);

        if (request.RoleIds?.Any() == true)
        {
            foreach (var roleId in request.RoleIds)
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId, AssignedBy = _currentUserService.UserId });
            }
        }

        await _context.SaveChangesAsync();
        return await GetUserAsync(user.Id);
    }

    public async Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        if (request.Email != null) user.Email = request.Email;
        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Phone != null) user.Phone = request.Phone;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
        if (!string.IsNullOrEmpty(request.NewPassword)) user.PasswordHash = HashPassword(request.NewPassword);

        await _context.SaveChangesAsync();
        return await GetUserAsync(id);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = _currentUserService.UserId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId)
    {
        var user = await _context.Users.FindAsync(userId);
        var role = await _context.Roles.FindAsync(roleId);
        if (user == null || role == null) return false;

        if (await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId))
            return false;

        _context.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedBy = _currentUserService.UserId
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, Guid roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null) return false;

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .ToListAsync();

        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive,
            Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList()
        }).ToList();
    }

    public async Task<List<PermissionDto>> GetPermissionsAsync()
    {
        var permissions = await _context.Permissions.ToListAsync();
        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Group = p.Group
        }).ToList();
    }

    private async Task<AuthResult> GenerateAuthResult(User user, string? ipAddress, Guid? existingFamilyId = null)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var accessToken = GenerateAccessToken(user, roles, permissions);
        var familyId = existingFamilyId ?? Guid.NewGuid();
        var refreshToken = GenerateRefreshToken(user.Id, ipAddress, familyId);

        _context.RefreshTokens.Add(refreshToken);

        // Link the previous token in the family to the new one
        if (existingFamilyId.HasValue)
        {
            var previousTokens = await _context.RefreshTokens
                .Where(rt => rt.FamilyId == existingFamilyId.Value && rt.UserId == user.Id && rt.RevokedAt != null && rt.ReplacedByTokenId == null)
                .ToListAsync();
            foreach (var prev in previousTokens)
            {
                prev.ReplacedByTokenId = refreshToken.Id;
            }
        }

        await _context.SaveChangesAsync();

        return new AuthResult
        {
            IsSuccess = true,
            Token = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                Roles = roles,
                Permissions = permissions
            }
        };
    }

    private string GenerateAccessToken(User user, List<string> roles, List<string> permissions)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.Username),
            new("username", user.Username),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress, Guid familyId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            FamilyId = familyId
        };
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    private static UserDto MapToDto(User user)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            Roles = roles,
            Permissions = permissions
        };
    }
}
