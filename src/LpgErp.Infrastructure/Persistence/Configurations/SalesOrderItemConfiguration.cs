using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable("SalesOrderItems");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.UnitPrice).HasPrecision(18, 2);
        builder.HasOne(s => s.SalesOrder).WithMany(so => so.Items).HasForeignKey(s => s.SalesOrderId);
        builder.HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId);
    }
}
