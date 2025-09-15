using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Common;

namespace TheDugout.Data.Configurations
{
    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasIndex(r => r.Code).IsUnique();

            builder.Property(r => r.Code).IsRequired().HasMaxLength(3);

            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        }
    }
}
