using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class PlayerAttributeConfiguration : IEntityTypeConfiguration<PlayerAttribute>
    {
        public void Configure(EntityTypeBuilder<PlayerAttribute> builder)
        {
            builder.ToTable("PlayerAttributes");

            builder.HasKey(pa => pa.Id);

            builder.Property(pa => pa.Value)
                   .IsRequired();

            builder.HasOne(pa => pa.Player)
                   .WithMany(p => p.Attributes)
                   .HasForeignKey(pa => pa.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pa => pa.Attribute)
                   .WithMany(a => a.PlayerAttributes)
                   .HasForeignKey(pa => pa.AttributeId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pa => new { pa.PlayerId, pa.AttributeId })
                   .IsUnique();
        }
    }
}
