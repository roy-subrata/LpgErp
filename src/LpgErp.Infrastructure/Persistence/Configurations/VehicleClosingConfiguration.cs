using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class VehicleClosingConfiguration : IEntityTypeConfiguration<VehicleClosing>
{
    public void Configure(EntityTypeBuilder<VehicleClosing> builder)
    {
        builder.ToTable("VehicleClosings");
        builder.HasKey(v => v.Id);
        builder.HasOne(v => v.VehicleLoading).WithMany().HasForeignKey(v => v.VehicleLoadingId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(v => v.CashCollected).HasPrecision(18, 2);
        builder.Property(v => v.CreditSales).HasPrecision(18, 2);
        builder.Property(v => v.OutstandingAmount).HasPrecision(18, 2);
    }
}

public class VehicleClosingItemConfiguration : IEntityTypeConfiguration<VehicleClosingItem>
{
    public void Configure(EntityTypeBuilder<VehicleClosingItem> builder)
    {
        builder.ToTable("VehicleClosingItems");
        builder.HasKey(v => v.Id);
        builder.HasOne(v => v.VehicleClosing).WithMany(x => x.Items).HasForeignKey(v => v.VehicleClosingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Product).WithMany().HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
