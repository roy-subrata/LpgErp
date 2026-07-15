using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.UnitPrice).HasPrecision(18, 2);
        builder.HasOne(p => p.PurchaseOrder).WithMany(po => po.Items).HasForeignKey(p => p.PurchaseOrderId);
        builder.HasOne(p => p.Product).WithMany().HasForeignKey(p => p.ProductId);
    }
}
