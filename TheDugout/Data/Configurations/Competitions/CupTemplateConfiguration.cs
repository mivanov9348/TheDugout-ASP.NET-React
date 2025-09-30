using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Cups;

namespace TheDugout.Data.Configurations
{
    public class CupTemplateConfiguration : IEntityTypeConfiguration<CupTemplate>
    {
        public void Configure(EntityTypeBuilder<CupTemplate> builder)
        {
            builder.HasKey(ct => ct.Id);

            builder.Property(ct => ct.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ct => ct.CountryCode)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}
