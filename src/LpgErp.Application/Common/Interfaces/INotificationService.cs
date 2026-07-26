namespace LpgErp.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifySaleCreatedAsync(string orderNumber, string customerName, decimal amount, Guid? customerId);
    Task NotifyPaymentReceivedAsync(string orderNumber, decimal amount, string method, Guid? customerId);
    Task NotifyPaymentOutboundAsync(string orderNumber, decimal amount, string method);
    Task NotifyStockLowAsync(string productName, int currentStock, int minimumStock);
    Task NotifyToGroupAsync(string group, string method, object[] args);
    Task NotifyToUserAsync(string userId, string method, object[] args);
    Task NotifyAllAsync(string method, object[] args);
}

public class RealTimeNotification
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Severity { get; set; }
    public string[]? TargetRoles { get; set; }
}
