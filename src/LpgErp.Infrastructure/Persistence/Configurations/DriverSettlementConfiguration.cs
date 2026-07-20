using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class DriverSettlementConfiguration : IEntityTypeConfiguration<DriverSettlement>
{
    public void Configure(EntityTypeBuilder<DriverSettlement> builder)
    {
        builder.ToTable("DriverSettlements");
        builder.HasKey(d => d.Id);
        builder.HasOne(d => d.Driver).WithMany().HasForeignKey(d => d.DriverId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.VehicleLoading).WithMany().HasForeignKey(d => d.VehicleLoadingId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(d => d.FuelCost).HasPrecision(18, 2);
        builder.Property(d => d.Allowance).HasPrecision(18, 2);
        builder.Property(d => d.LoadingCost).HasPrecision(18, 2);
        builder.Property(d => d.UnloadingCost).HasPrecision(18, 2);
        builder.Property(d => d.TripIncome).HasPrecision(18, 2);
        builder.Property(d => d.CompanyPickupIncentive).HasPrecision(18, 2);
    }
}
