using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class CustomerCylinderBalanceConfiguration : IEntityTypeConfiguration<CustomerCylinderBalance>
{
    public void Configure(EntityTypeBuilder<CustomerCylinderBalance> builder)
    {
        builder.ToTable("CustomerCylinderBalances");
        builder.HasKey(c => c.Id);
        builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Brand).WithMany().HasForeignKey(c => c.BrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.CylinderSize).WithMany().HasForeignKey(c => c.CylinderSizeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => new { c.CustomerId, c.BrandId, c.CylinderSizeId }).IsUnique();
    }
}
