using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Players;

namespace TheDugout.Data.Configurations
{
    public class PlayerAttributesConfiguration : IEntityTypeConfiguration<PlayerAttribute>
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
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pa => pa.Attribute)
                   .WithMany(a => a.PlayerAttributes)
                   .HasForeignKey(pa => pa.AttributeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pa => new { pa.PlayerId, pa.AttributeId })
                   .IsUnique();
        }
    }
}
