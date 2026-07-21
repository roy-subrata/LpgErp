using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Trucks.DTOs;

public class TruckDto : IMapFrom<Truck>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTruckRequest : IMapTo<Truck>
{
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTruckRequest : IMapTo<Truck>
{
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
