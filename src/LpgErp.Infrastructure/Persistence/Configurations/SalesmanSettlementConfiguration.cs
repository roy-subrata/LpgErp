using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class SalesmanSettlementConfiguration : IEntityTypeConfiguration<SalesmanSettlement>
{
    public void Configure(EntityTypeBuilder<SalesmanSettlement> builder)
    {
        builder.ToTable("SalesmanSettlements");
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Salesman).WithMany().HasForeignKey(s => s.SalesmanId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.TotalSales).HasPrecision(18, 2);
        builder.Property(s => s.Collection).HasPrecision(18, 2);
        builder.Property(s => s.Commission).HasPrecision(18, 2);
        builder.Property(s => s.DailyAllowance).HasPrecision(18, 2);
        builder.Property(s => s.Bonus).HasPrecision(18, 2);
    }
}
