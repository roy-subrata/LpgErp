using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Payments.DTOs;

public class PaymentDto : IMapFrom<Payment>
{
    public Guid Id { get; set; }
    public Guid? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.SalesOrderNumber, opt => opt.MapFrom(s => s.SalesOrder != null ? s.SalesOrder.OrderNumber : null))
            .ForMember(d => d.PurchaseOrderNumber, opt => opt.MapFrom(s => s.PurchaseOrder != null ? s.PurchaseOrder.OrderNumber : null));
    }
}

public class UpdatePaymentRequest
{
    public Guid? SalesOrderId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class CreatePaymentRequest
{
    public Guid? SalesOrderId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
