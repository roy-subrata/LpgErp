using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class CylinderConfiguration : IEntityTypeConfiguration<Cylinder>
{
    public void Configure(EntityTypeBuilder<Cylinder> builder)
    {
        builder.ToTable("Cylinders");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.SerialNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.SerialNumber).IsUnique();
        builder.HasOne(c => c.Brand).WithMany().HasForeignKey(c => c.BrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.CylinderSize).WithMany().HasForeignKey(c => c.CylinderSizeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.CurrentWarehouse).WithMany().HasForeignKey(c => c.CurrentWarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}
