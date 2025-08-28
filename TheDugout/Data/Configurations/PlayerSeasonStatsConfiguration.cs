using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class PlayerSeasonStatsConfiguration : IEntityTypeConfiguration<PlayerSeasonStats>
    {
        public void Configure(EntityTypeBuilder<PlayerSeasonStats> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.SeasonStats)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Season)
                   .WithMany(s => s.PlayerStats)
                   .HasForeignKey(e => e.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
