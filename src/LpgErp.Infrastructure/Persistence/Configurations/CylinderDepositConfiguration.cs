using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class CylinderDepositConfiguration : IEntityTypeConfiguration<CylinderDeposit>
{
    public void Configure(EntityTypeBuilder<CylinderDeposit> builder)
    {
        builder.ToTable("CylinderDeposits");
        builder.HasKey(c => c.Id);
        builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.CylinderSize).WithMany().HasForeignKey(c => c.CylinderSizeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(c => c.Amount).HasPrecision(18, 2);
    }
}
