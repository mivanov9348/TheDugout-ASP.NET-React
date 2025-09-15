using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TheDugout.Data.Configurations
{
    public class AttributeConfiguration : IEntityTypeConfiguration<Models.Players.Attribute>
    {
        public void Configure(EntityTypeBuilder<Models.Players.Attribute> builder)
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
