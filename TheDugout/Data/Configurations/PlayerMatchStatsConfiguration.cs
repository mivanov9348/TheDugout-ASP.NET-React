using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Players;

namespace TheDugout.Data.Configurations
{
    public class PlayerMatchStatsConfiguration : IEntityTypeConfiguration<PlayerMatchStats>
    {
        public void Configure(EntityTypeBuilder<PlayerMatchStats> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.MatchStats)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
