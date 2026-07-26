using LpgErp.Application.Common.Interfaces;

namespace LpgErp.Api.Tests.Unit;

internal class NullNotificationService : INotificationService
{
    public Task NotifySaleCreatedAsync(string orderNumber, string customerName, decimal amount, Guid? customerId) => Task.CompletedTask;
    public Task NotifyPaymentReceivedAsync(string orderNumber, decimal amount, string method, Guid? customerId) => Task.CompletedTask;
    public Task NotifyPaymentOutboundAsync(string orderNumber, decimal amount, string method) => Task.CompletedTask;
    public Task NotifyStockLowAsync(string productName, int currentStock, int minimumStock) => Task.CompletedTask;
    public Task NotifyToGroupAsync(string group, string method, object[] args) => Task.CompletedTask;
    public Task NotifyToUserAsync(string userId, string method, object[] args) => Task.CompletedTask;
    public Task NotifyAllAsync(string method, object[] args) => Task.CompletedTask;
}
