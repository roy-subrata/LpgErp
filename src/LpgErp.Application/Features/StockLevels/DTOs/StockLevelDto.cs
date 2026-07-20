using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;

namespace LpgErp.Application.Features.StockLevels.DTOs;

public class StockLevelDto : IMapFrom<StockLevel>
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<StockLevel, StockLevelDto>()
            .ForMember(d => d.WarehouseName, opt => opt.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));
    }
}
