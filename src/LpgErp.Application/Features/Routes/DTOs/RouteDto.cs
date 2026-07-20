using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Routes.DTOs;

public class RouteDto : IMapFrom<Route>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? Description { get; set; }
    public string? Village { get; set; }
    public string? Dealer { get; set; }
    public bool IsActive { get; set; }
}

public class CreateRouteRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? Description { get; set; }
    public string? Village { get; set; }
    public string? Dealer { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateRouteRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? Description { get; set; }
    public string? Village { get; set; }
    public string? Dealer { get; set; }
    public bool IsActive { get; set; } = true;
}
