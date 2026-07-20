using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class DailySalesSummaryConfiguration : IEntityTypeConfiguration<DailySalesSummary>
{
    public void Configure(EntityTypeBuilder<DailySalesSummary> builder)
    {
        builder.ToTable("DailySalesSummaries");
        builder.HasKey(d => d.Id);
        builder.HasOne(d => d.VehicleLoading).WithMany().HasForeignKey(d => d.VehicleLoadingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Truck).WithMany().HasForeignKey(d => d.TruckId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Driver).WithMany().HasForeignKey(d => d.DriverId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Salesman).WithMany().HasForeignKey(d => d.SalesmanId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(d => d.TotalSales).HasPrecision(18, 2);
        builder.Property(d => d.CashSales).HasPrecision(18, 2);
        builder.Property(d => d.CreditSales).HasPrecision(18, 2);
        builder.Property(d => d.PaymentsCollected).HasPrecision(18, 2);
        builder.Property(d => d.DueCreated).HasPrecision(18, 2);
    }
}
