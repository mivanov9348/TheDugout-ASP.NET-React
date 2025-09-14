
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Common;

namespace TheDugout.Data.Configurations
{
    public class FirstNameConfiguration : IEntityTypeConfiguration<FirstName>
    {
        public void Configure(EntityTypeBuilder<FirstName> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.Name).IsRequired().HasMaxLength(100);

            builder.HasOne(f => f.Region)
                   .WithMany(r => r.FirstNames)
                   .HasForeignKey(f => f.RegionCode)
                   .HasPrincipalKey(r => r.Code);
        }
    }
}
