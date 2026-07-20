using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.CustomerNotifications.DTOs;

public class CustomerNotificationDto : IMapFrom<CustomerNotification>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsSent { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CustomerNotification, CustomerNotificationDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name));
    }
}

public class CreateCustomerNotificationRequest
{
    public Guid CustomerId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class UpdateCustomerNotificationRequest
{
    public Guid CustomerId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
