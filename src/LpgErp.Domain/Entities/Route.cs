namespace LpgErp.Domain.Entities;

public class Route : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? Village { get; set; }
    public string? Dealer { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
