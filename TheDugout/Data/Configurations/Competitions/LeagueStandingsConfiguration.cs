using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Leagues;

namespace TheDugout.Data.Configurations
{
    public class LeagueStandingConfiguration : IEntityTypeConfiguration<LeagueStanding>
    {
        public void Configure(EntityTypeBuilder<LeagueStanding> builder)
        {
            builder.ToTable("LeagueStandings");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.LeagueId, e.TeamId })
                   .IsUnique();

            builder.Property(e => e.Points).HasDefaultValue(0);
            builder.Property(e => e.Matches).HasDefaultValue(0);
            builder.Property(e => e.Wins).HasDefaultValue(0);
            builder.Property(e => e.Draws).HasDefaultValue(0);
            builder.Property(e => e.Losses).HasDefaultValue(0);
            builder.Property(e => e.GoalsFor).HasDefaultValue(0);
            builder.Property(e => e.GoalsAgainst).HasDefaultValue(0);
            builder.Property(e => e.GoalDifference).HasDefaultValue(0);
            builder.Property(e => e.Ranking).HasDefaultValue(0);

            builder.HasOne(e => e.GameSave)
                   .WithMany(gs => gs.LeagueStandings)
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Season)
                   .WithMany(s => s.LeagueStandings)
                   .HasForeignKey(e => e.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.League)
                   .WithMany(l => l.Standings)
                   .HasForeignKey(e => e.LeagueId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Team)
                   .WithMany(t => t.LeagueStandings)
                   .HasForeignKey(e => e.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
