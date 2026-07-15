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
        builder.HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId);
        builder.HasOne(s => s.FromWarehouse).WithMany().HasForeignKey(s => s.FromWarehouseId);
        builder.HasOne(s => s.ToWarehouse).WithMany().HasForeignKey(s => s.ToWarehouseId);
        builder.HasOne(s => s.PurchaseOrder).WithMany().HasForeignKey(s => s.PurchaseOrderId);
        builder.HasOne(s => s.SalesOrder).WithMany().HasForeignKey(s => s.SalesOrderId);
    }
}
