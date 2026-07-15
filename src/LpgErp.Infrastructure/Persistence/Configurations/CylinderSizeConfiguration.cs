using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class CylinderSizeConfiguration : IEntityTypeConfiguration<CylinderSize>
{
    public void Configure(EntityTypeBuilder<CylinderSize> builder)
    {
        builder.ToTable("CylinderSizes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.WeightKg).HasPrecision(10, 2);
        builder.Property(c => c.DepositAmount).HasPrecision(18, 2);
        builder.HasOne(c => c.Brand).WithMany(b => b.CylinderSizes).HasForeignKey(c => c.BrandId);
    }
}
