namespace LpgErp.Domain.Entities;

public class SystemNotification : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string? Severity { get; set; }
    public string TargetRoles { get; set; } = "[]";
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
