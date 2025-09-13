using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Players;

namespace TheDugout.Data.Configurations
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.ToTable("Positions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Code)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(p => p.Code)
                   .IsUnique(); 
        }
    }
}
