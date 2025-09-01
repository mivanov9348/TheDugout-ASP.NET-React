using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class FixtureConfiguration : IEntityTypeConfiguration<Fixture>
    {
        public void Configure(EntityTypeBuilder<Fixture> builder)
        {
            builder.HasKey(f => f.Id);


            builder.HasOne(f => f.GameSave)
       .WithMany(gs => gs.Fixtures)
       .HasForeignKey(f => f.GameSaveId)
       .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.League)
                   .WithMany(l => l.Fixtures)
                   .HasForeignKey(f => f.LeagueId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Season)
       .WithMany(s => s.Fixtures)
       .HasForeignKey(f => f.SeasonId)
       .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(f => f.HomeTeam)
                   .WithMany(t => t.HomeFixtures)
                   .HasForeignKey(f => f.HomeTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.AwayTeam)
                   .WithMany(t => t.AwayFixtures)
                   .HasForeignKey(f => f.AwayTeamId)
                   .OnDelete(DeleteBehavior.Restrict);




        }

    }
}
