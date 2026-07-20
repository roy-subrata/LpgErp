using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.FromWarehouse).WithMany().HasForeignKey(s => s.FromWarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.ToWarehouse).WithMany().HasForeignKey(s => s.ToWarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.PurchaseOrder).WithMany().HasForeignKey(s => s.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.SalesOrder).WithMany().HasForeignKey(s => s.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
    }
}
