using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.HasOne(p => p.SalesOrder).WithMany(s => s.Payments).HasForeignKey(p => p.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.PurchaseOrder).WithMany(po => po.Payments).HasForeignKey(p => p.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
    }
}
