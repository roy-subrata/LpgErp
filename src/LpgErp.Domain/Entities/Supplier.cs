namespace LpgErp.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsLpgCompany { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }
}
