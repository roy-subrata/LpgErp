namespace LpgErp.Domain.Entities;

public enum NotificationType
{
    PaymentDue = 0,
    PossibleRefillTime = 1,
    CylinderReturnReminder = 2,
    OutstandingEmptyCylinder = 3,
    CreditLimitExceeded = 4
}

public class CustomerNotification : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
}
