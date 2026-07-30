using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LpgErp.Infrastructure.Persistence.Configurations;

public class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
{
    public void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        builder.ToTable("CompanySettings");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Website).HasMaxLength(200);
    }
}
