using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Fixtures;

namespace TheDugout.Data.Configurations
{
    public class FixtureConfiguration : IEntityTypeConfiguration<Fixture>
    {
        public void Configure(EntityTypeBuilder<Fixture> builder)
        {
            builder.Property(f => f.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(f => f.HomeTeamGoals)
                   .HasDefaultValue(0);

            builder.Property(f => f.AwayTeamGoals)
                   .HasDefaultValue(0);

            builder.HasOne(f => f.GameSave)
                   .WithMany(gs => gs.Fixtures)
                   .HasForeignKey(f => f.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Season)
                   .WithMany(s => s.Fixtures)
                   .HasForeignKey(f => f.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.League)
                   .WithMany(l => l.Fixtures)
                   .HasForeignKey(f => f.LeagueId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.CupRound)
                   .WithMany(cr => cr.Fixtures)
                   .HasForeignKey(f => f.CupRoundId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.EuropeanCupPhase)
                   .WithMany(p => p.Fixtures)
                   .HasForeignKey(f => f.EuropeanCupPhaseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.HomeTeam)
                   .WithMany(t => t.HomeFixtures)
                   .HasForeignKey(f => f.HomeTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.AwayTeam)
                   .WithMany(t => t.AwayFixtures)
                   .HasForeignKey(f => f.AwayTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.WinnerTeam)
                   .WithMany()
                   .HasForeignKey(f => f.WinnerTeamId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
