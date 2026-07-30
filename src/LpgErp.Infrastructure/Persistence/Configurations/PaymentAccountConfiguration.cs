using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class PaymentAccountConfiguration : IEntityTypeConfiguration<PaymentAccount>
{
    public void Configure(EntityTypeBuilder<PaymentAccount> builder)
    {
        builder.ToTable("PaymentAccounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.AccountNumber).HasMaxLength(50);
        builder.Property(a => a.Provider).HasMaxLength(100);
        builder.Property(a => a.Notes).HasMaxLength(500);

        // Filtered so a soft-deleted account doesn't permanently reserve its name — deletes here
        // are soft (see SaveChangesAsync), and the name should be reusable afterwards.
        builder.HasIndex(a => a.Name).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
