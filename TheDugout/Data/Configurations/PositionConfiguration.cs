using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Code)
                   .IsRequired()
                   .HasMaxLength(5);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasMany(p => p.Players)
                   .WithOne(pl => pl.Position)
                   .HasForeignKey(pl => pl.PositionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
