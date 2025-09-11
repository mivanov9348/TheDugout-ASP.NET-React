using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class EuropeanCupStandingConfiguration : IEntityTypeConfiguration<EuropeanCupStanding>
    {
        public void Configure(EntityTypeBuilder<EuropeanCupStanding> builder)
        {
            builder.ToTable("EuropeanCupStandings");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.EuropeanCupId, e.TeamId }).IsUnique();

            builder.Property(e => e.Points).HasDefaultValue(0);
            builder.Property(e => e.Matches).HasDefaultValue(0);
            builder.Property(e => e.Wins).HasDefaultValue(0);
            builder.Property(e => e.Draws).HasDefaultValue(0);
            builder.Property(e => e.Losses).HasDefaultValue(0);
            builder.Property(e => e.GoalsFor).HasDefaultValue(0);
            builder.Property(e => e.GoalsAgainst).HasDefaultValue(0);
            builder.Property(e => e.GoalDifference).HasDefaultValue(0);
            builder.Property(e => e.Ranking).HasDefaultValue(0);

            builder.HasOne(e => e.EuropeanCup)
                   .WithMany(c => c.Standings)
                   .HasForeignKey(e => e.EuropeanCupId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Team)
                   .WithMany(t => t.EuropeanCupStandings)
                   .HasForeignKey(e => e.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
