using System.Text.Json;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Domain.Entities;
using LpgErp.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LpgErp.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NotificationHub> hubContext, IApplicationDbContext context, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _context = context;
        _logger = logger;
    }

    public async Task NotifySaleCreatedAsync(string orderNumber, string customerName, decimal amount, Guid? customerId)
    {
        var roles = new[] { "Admin", "Manager" };
        var notification = new RealTimeNotification
        {
            Type = "SaleCreated",
            Title = "New Sale",
            Message = $"Order {orderNumber} created for {customerName} - ৳{amount:N2}",
            EntityType = "SalesOrder",
            Severity = "info",
            TargetRoles = roles
        };

        await PersistAsync(notification, roles);
        await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification", notification);
        await _hubContext.Clients.Group("role_Manager").SendAsync("ReceiveNotification", notification);

        if (customerId.HasValue)
        {
            await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("ReceiveNotification", notification);
        }

        _logger.LogInformation("Sale notification sent: {OrderNumber}", orderNumber);
    }

    public async Task NotifyPaymentReceivedAsync(string orderNumber, decimal amount, string method, Guid? customerId)
    {
        var roles = new[] { "Admin", "Manager", "Accountant" };
        var notification = new RealTimeNotification
        {
            Type = "PaymentReceived",
            Title = "Payment Received",
            Message = $"৳{amount:N2} received for {orderNumber} via {method}",
            EntityType = "Payment",
            Severity = "success",
            TargetRoles = roles
        };

        await PersistAsync(notification, roles);
        await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification", notification);
        await _hubContext.Clients.Group("role_Manager").SendAsync("ReceiveNotification", notification);
        await _hubContext.Clients.Group("role_Accountant").SendAsync("ReceiveNotification", notification);

        if (customerId.HasValue)
        {
            await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("ReceiveNotification", notification);
        }

        _logger.LogInformation("Payment notification sent: {OrderNumber} ৳{Amount}", orderNumber, amount);
    }

    public async Task NotifyPaymentOutboundAsync(string orderNumber, decimal amount, string method)
    {
        var roles = new[] { "Admin", "Manager", "Accountant" };
        var notification = new RealTimeNotification
        {
            Type = "PaymentOutbound",
            Title = "Payment Sent",
            Message = $"৳{amount:N2} paid for {orderNumber} via {method}",
            EntityType = "Payment",
            Severity = "warning",
            TargetRoles = roles
        };

        await PersistAsync(notification, roles);
        await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification", notification);
        await _hubContext.Clients.Group("role_Manager").SendAsync("ReceiveNotification", notification);
        await _hubContext.Clients.Group("role_Accountant").SendAsync("ReceiveNotification", notification);

        _logger.LogInformation("Outbound payment notification sent: {OrderNumber} ৳{Amount}", orderNumber, amount);
    }

    public async Task NotifyStockLowAsync(string productName, int currentStock, int minimumStock)
    {
        var roles = new[] { "Admin", "Manager", "Warehouse" };
        var notification = new RealTimeNotification
        {
            Type = "StockLow",
            Title = "Low Stock Alert",
            Message = $"{productName} is low on stock ({currentStock}/{minimumStock})",
            EntityType = "Product",
            Severity = "danger",
            TargetRoles = roles
        };

        await PersistAsync(notification, roles);
        await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification", notification);
        await _hubContext.Clients.Group("role_Manager").SendAsync("ReceiveNotification", notification);
        await _hubContext.Clients.Group("role_Warehouse").SendAsync("ReceiveNotification", notification);

        _logger.LogWarning("Low stock alert: {Product} ({Current}/{Min})", productName, currentStock, minimumStock);
    }

    public async Task NotifyToGroupAsync(string group, string method, object[] args)
    {
        await _hubContext.Clients.Group(group).SendAsync(method, args);
    }

    public async Task NotifyToUserAsync(string userId, string method, object[] args)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync(method, args);
    }

    public async Task NotifyAllAsync(string method, object[] args)
    {
        await _hubContext.Clients.Group("all").SendAsync(method, args);
    }

    private async Task PersistAsync(RealTimeNotification notification, string[] targetRoles)
    {
        try
        {
            var entity = new SystemNotification
            {
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                EntityId = notification.EntityId,
                EntityType = notification.EntityType,
                Severity = notification.Severity,
                TargetRoles = JsonSerializer.Serialize(targetRoles)
            };
            _context.SystemNotifications.Add(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist notification");
        }
    }
}
