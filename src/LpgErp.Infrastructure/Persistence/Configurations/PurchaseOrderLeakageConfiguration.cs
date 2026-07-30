using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class PurchaseOrderLeakageConfiguration : IEntityTypeConfiguration<PurchaseOrderLeakage>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLeakage> builder)
    {
        builder.ToTable("PurchaseOrderLeakages");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.CreditAmount).HasPrecision(18, 2);
        builder.Property(l => l.Notes).HasMaxLength(500);

        builder.HasOne(l => l.PurchaseOrder).WithMany(po => po.Leakages)
            .HasForeignKey(l => l.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Brand).WithMany()
            .HasForeignKey(l => l.BrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.CylinderSize).WithMany()
            .HasForeignKey(l => l.CylinderSizeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.PurchaseOrderId);
    }
}
