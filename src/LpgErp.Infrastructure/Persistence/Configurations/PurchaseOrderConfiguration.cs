using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(p => p.TotalAmount).HasPrecision(18, 2);
        builder.Property(p => p.CommissionEarned).HasPrecision(18, 2);
        builder.HasOne(p => p.Supplier).WithMany().HasForeignKey(p => p.SupplierId);
        builder.HasOne(p => p.Warehouse).WithMany().HasForeignKey(p => p.WarehouseId);
        builder.HasIndex(p => p.OrderNumber).IsUnique();
    }
}
