using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class AttributeConfiguration : IEntityTypeConfiguration<Models.Attribute>
    {
        public void Configure(EntityTypeBuilder<Models.Attribute> builder)
        {
            builder.ToTable("Attributes");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Code)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(a => a.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(a => a.Code)
                   .IsUnique(); 
        }
    }
}
