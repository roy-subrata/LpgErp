using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class VehicleLoadingConfiguration : IEntityTypeConfiguration<VehicleLoading>
{
    public void Configure(EntityTypeBuilder<VehicleLoading> builder)
    {
        builder.ToTable("VehicleLoadings");
        builder.HasKey(v => v.Id);
        builder.HasOne(v => v.Truck).WithMany().HasForeignKey(v => v.TruckId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Driver).WithMany().HasForeignKey(v => v.DriverId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Salesman).WithMany().HasForeignKey(v => v.SalesmanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Warehouse).WithMany().HasForeignKey(v => v.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Route).WithMany().HasForeignKey(v => v.RouteId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class VehicleLoadingItemConfiguration : IEntityTypeConfiguration<VehicleLoadingItem>
{
    public void Configure(EntityTypeBuilder<VehicleLoadingItem> builder)
    {
        builder.ToTable("VehicleLoadingItems");
        builder.HasKey(v => v.Id);
        builder.HasOne(v => v.VehicleLoading).WithMany(x => x.Items).HasForeignKey(v => v.VehicleLoadingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Product).WithMany().HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
