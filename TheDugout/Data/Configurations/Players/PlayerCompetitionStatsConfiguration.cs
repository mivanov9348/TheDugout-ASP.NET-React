namespace TheDugout.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Players;

    public class PlayerCompetitionStatsConfiguration : IEntityTypeConfiguration<PlayerCompetitionStats>
    {
        public void Configure(EntityTypeBuilder<PlayerCompetitionStats> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.CompetitionStats)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Competition)
                   .WithMany(c => c.PlayerCompetitionStats)
                   .HasForeignKey(e => e.CompetitionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Season)
                   .WithMany(s => s.PlayerCompetitionStats)
                   .HasForeignKey(e => e.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.GameSave)
                   .WithMany(gs => gs.PlayerCompetitionStats)
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.MatchesPlayed)
                   .HasDefaultValue(0);

            builder.Property(e => e.Goals)
                   .HasDefaultValue(0);
        }
    }
}
