using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class CommissionLedgerConfiguration : IEntityTypeConfiguration<CommissionLedger>
{
    public void Configure(EntityTypeBuilder<CommissionLedger> builder)
    {
        builder.ToTable("CommissionLedgers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ActualAmount).HasPrecision(18, 2);
        builder.Property(c => c.CommissionEarned).HasPrecision(18, 2);
        builder.Property(c => c.PeriodKey).HasMaxLength(50);
        builder.HasOne(c => c.Policy).WithMany().HasForeignKey(c => c.PolicyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => new { c.EntityType, c.EntityId, c.PeriodKey, c.PolicyId }).IsUnique();
    }
}
