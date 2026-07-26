using LpgErp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LpgErp.Infrastructure.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentUserService _currentUserService;

    public NotificationHub(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        var roles = _currentUserService.Roles.ToList();
        foreach (var role in roles)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role}");

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            }
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, "all");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
