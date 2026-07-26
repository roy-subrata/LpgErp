using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.PriceHistories.DTOs;

public class PriceHistoryDto : IMapFrom<PriceHistory>
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public PriceType PriceType { get; set; }
    public decimal PreviousPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal? BoardPrice { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal ChangePercent { get; set; }
    public PriceChangeReason Reason { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<PriceHistory, PriceHistoryDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));
    }
}

public class CreatePriceHistoryRequest : IMapTo<PriceHistory>
{
    public Guid ProductId { get; set; }
    public PriceType PriceType { get; set; }
    public decimal NewPrice { get; set; }
    public decimal? BoardPrice { get; set; }
    public PriceChangeReason Reason { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
