using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(s => s.TotalAmount).HasPrecision(18, 2);
        builder.Property(s => s.Discount).HasPrecision(18, 2);
        builder.HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId);
        builder.HasOne(s => s.Warehouse).WithMany().HasForeignKey(s => s.WarehouseId);
        builder.HasIndex(s => s.OrderNumber).IsUnique();
    }
}
