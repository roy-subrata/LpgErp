using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class CommissionPolicyConfiguration : IEntityTypeConfiguration<CommissionPolicy>
{
    public void Configure(EntityTypeBuilder<CommissionPolicy> builder)
    {
        builder.ToTable("CommissionPolicies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.CommissionValue).HasPrecision(18, 2);
        builder.HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Brand).WithMany().HasForeignKey(c => c.BrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.CylinderSize).WithMany().HasForeignKey(c => c.CylinderSizeId).OnDelete(DeleteBehavior.Restrict);
    }
}
