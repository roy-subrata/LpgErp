using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Code).HasMaxLength(50);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.Email).HasMaxLength(200);
        builder.Property(s => s.CommissionPerCylinder).HasPrecision(18, 2);
        builder.Property(s => s.CommissionBalance).HasPrecision(18, 2);
        builder.HasOne(s => s.Brand).WithMany(b => b.Suppliers).HasForeignKey(s => s.BrandId);
    }
}
