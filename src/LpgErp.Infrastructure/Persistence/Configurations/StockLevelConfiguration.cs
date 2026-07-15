using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.ToTable("StockLevels");
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Warehouse).WithMany().HasForeignKey(s => s.WarehouseId);
        builder.HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId);
        builder.HasIndex(s => new { s.WarehouseId, s.ProductId }).IsUnique();
    }
}
