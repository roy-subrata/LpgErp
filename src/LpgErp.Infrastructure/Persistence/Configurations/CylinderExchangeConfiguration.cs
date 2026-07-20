using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class CylinderExchangeConfiguration : IEntityTypeConfiguration<CylinderExchange>
{
    public void Configure(EntityTypeBuilder<CylinderExchange> builder)
    {
        builder.ToTable("CylinderExchanges");
        builder.HasKey(c => c.Id);
        builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.SalesOrder).WithMany().HasForeignKey(c => c.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.IncomingBrand).WithMany().HasForeignKey(c => c.IncomingBrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.IncomingCylinderSize).WithMany().HasForeignKey(c => c.IncomingCylinderSizeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.OutgoingBrand).WithMany().HasForeignKey(c => c.OutgoingBrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.OutgoingCylinderSize).WithMany().HasForeignKey(c => c.OutgoingCylinderSizeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(c => c.ExchangeCharge).HasPrecision(18, 2);
    }
}
