namespace TheDugout.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Players;
    public class PlayerMatchStatsConfiguration : IEntityTypeConfiguration<PlayerMatchStats>
    {
        public void Configure(EntityTypeBuilder<PlayerMatchStats> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.MatchStats)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Match)
                   .WithMany(m => m.PlayerStats)
                   .HasForeignKey(e => e.MatchId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ps => ps.Competition)
                   .WithMany(c => c.PlayerStats)
                   .HasForeignKey(ps => ps.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ps => ps.GameSave)
                   .WithMany(gs => gs.PlayerMatchStats)
                   .HasForeignKey(ps => ps.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);


        }

    }
}
