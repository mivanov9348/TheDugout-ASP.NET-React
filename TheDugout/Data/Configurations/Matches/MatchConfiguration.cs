namespace TheDugout.Data.Configurations.Matches
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Models.Matches;
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasOne(m => m.GameSave)
                   .WithMany(gs => gs.Matches)
                   .HasForeignKey(m => m.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Fixture)
               .WithOne(f => f.Match)
               .HasForeignKey<Match>(m => m.FixtureId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(m => m.Status).HasConversion<int>();
            builder.Property(m => m.CurrentTurn).HasConversion<int>();

            builder.HasMany(m => m.PlayerStats)
                   .WithOne(ps => ps.Match)
                   .HasForeignKey(ps => ps.MatchId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(m => m.Penalties)
                   .WithOne(e => e.Match)
                   .HasForeignKey(e => e.MatchId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Competition)
                   .WithMany(c => c.Matches)
                   .HasForeignKey(m => m.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
