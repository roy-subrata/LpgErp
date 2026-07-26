using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.ToTable("PriceHistories");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PreviousPrice).HasPrecision(18, 2);
        builder.Property(p => p.NewPrice).HasPrecision(18, 2);
        builder.HasOne(p => p.Product).WithMany().HasForeignKey(p => p.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
