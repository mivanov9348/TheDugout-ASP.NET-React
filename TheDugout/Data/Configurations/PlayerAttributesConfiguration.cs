using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class PlayerAttributesConfiguration : IEntityTypeConfiguration<PlayerAttributes>
    {
        public void Configure(EntityTypeBuilder<PlayerAttributes> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Player)
                   .WithOne(p => p.Attributes)
                   .HasForeignKey<PlayerAttributes>(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
