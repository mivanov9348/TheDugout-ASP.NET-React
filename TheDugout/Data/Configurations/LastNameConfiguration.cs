using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Common;

namespace TheDugout.Data.Configurations
{
    public class LastNameConfiguration : IEntityTypeConfiguration<LastName>
    {
        public void Configure(EntityTypeBuilder<LastName> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Name).IsRequired().HasMaxLength(100);

            builder.HasOne(l => l.Region)
                   .WithMany(r => r.LastNames)
                   .HasForeignKey(l => l.RegionCode)
                   .HasPrincipalKey(r => r.Code);
        }
    }
}
