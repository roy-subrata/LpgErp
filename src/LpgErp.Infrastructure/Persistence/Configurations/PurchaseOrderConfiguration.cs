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
        builder.Property(p => p.CommissionApplied).HasPrecision(18, 2);
        builder.HasOne(p => p.Supplier).WithMany().HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Warehouse).WithMany().HasForeignKey(p => p.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.TransportCompany).WithMany().HasForeignKey(p => p.TransportCompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(p => p.TransportationCost).HasPrecision(18, 2);
        builder.HasIndex(p => p.OrderNumber).IsUnique();
    }
}
