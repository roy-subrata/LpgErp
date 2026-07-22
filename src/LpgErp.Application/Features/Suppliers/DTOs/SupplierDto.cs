using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Suppliers.DTOs;

public class SupplierDto : IMapFrom<Supplier>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsLpgCompany { get; set; }
    public decimal CommissionPerCylinder { get; set; }
    public decimal CommissionBalance { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Supplier, SupplierDto>()
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Name : null));
    }
}

public class CreateSupplierRequest : IMapTo<Supplier>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsLpgCompany { get; set; }
    public decimal CommissionPerCylinder { get; set; }
    public Guid? BrandId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSupplierRequest : IMapTo<Supplier>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsLpgCompany { get; set; }
    public decimal CommissionPerCylinder { get; set; }
    public Guid? BrandId { get; set; }
    public bool IsActive { get; set; } = true;
}
