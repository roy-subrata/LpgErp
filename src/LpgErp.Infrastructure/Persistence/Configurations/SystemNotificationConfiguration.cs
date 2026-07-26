using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class SystemNotificationConfiguration : IEntityTypeConfiguration<SystemNotification>
{
    public void Configure(EntityTypeBuilder<SystemNotification> builder)
    {
        builder.ToTable("SystemNotifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Type).HasMaxLength(50).IsRequired();
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.EntityType).HasMaxLength(50);
        builder.Property(n => n.Severity).HasMaxLength(20);
        builder.Property(n => n.TargetRoles).HasMaxLength(500).IsRequired();
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.IsRead);
    }
}
