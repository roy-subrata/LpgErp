using System.Text.Json;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class SystemNotificationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public SystemNotificationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var query = _context.SystemNotifications
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new SystemNotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                EntityId = n.EntityId,
                EntityType = n.EntityType,
                Severity = n.Severity,
                TargetRoles = n.TargetRoles,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<SystemNotificationDto>>.OkPaginated(items,
            new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var n = await _context.SystemNotifications.FindAsync([id], ct);
        if (n == null) return NotFound();

        return Ok(ApiResponse<SystemNotificationDto>.Ok(MapToDto(n)));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var n = await _context.SystemNotifications.FindAsync([id], ct);
        if (n == null) return NotFound();

        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse.Ok("Marked as read."));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        await _context.SystemNotifications
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);

        return Ok(ApiResponse.Ok("All notifications marked as read."));
    }

    private static SystemNotificationDto MapToDto(Domain.Entities.SystemNotification n) => new()
    {
        Id = n.Id,
        Type = n.Type,
        Title = n.Title,
        Message = n.Message,
        EntityId = n.EntityId,
        EntityType = n.EntityType,
        Severity = n.Severity,
        TargetRoles = n.TargetRoles,
        IsRead = n.IsRead,
        ReadAt = n.ReadAt,
        CreatedAt = n.CreatedAt
    };
}

public class SystemNotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string? Severity { get; set; }
    public string TargetRoles { get; set; } = "[]";
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
