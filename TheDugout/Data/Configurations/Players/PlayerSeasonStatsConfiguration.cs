using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Players;

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

            builder.HasOne(e => e.Competition)
                   .WithMany()
                   .HasForeignKey(e => e.CompetitionId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(e => e.MatchesPlayed)
                   .HasDefaultValue(0);

            builder.Property(e => e.Goals)
                   .HasDefaultValue(0);
        }
    }
}
