using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class TransportCompanyConfiguration : IEntityTypeConfiguration<TransportCompany>
{
    public void Configure(EntityTypeBuilder<TransportCompany> builder)
    {
        builder.ToTable("TransportCompanies");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Phone).HasMaxLength(50);
        builder.Property(t => t.Email).HasMaxLength(200);
    }
}
