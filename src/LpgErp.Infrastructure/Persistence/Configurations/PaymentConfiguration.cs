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
        builder.HasOne(p => p.PaymentAccount).WithMany(a => a.Payments).HasForeignKey(p => p.PaymentAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Customer).WithMany().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.CylinderDeposit).WithMany().HasForeignKey(p => p.CylinderDepositId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.CylinderExchange).WithMany().HasForeignKey(p => p.CylinderExchangeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.PaymentAccountId);
        // The statement reads every payment for one customer, newest first.
        builder.HasIndex(p => new { p.CustomerId, p.PaymentDate });
    }
}
