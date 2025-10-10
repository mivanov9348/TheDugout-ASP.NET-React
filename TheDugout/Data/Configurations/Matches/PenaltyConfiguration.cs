namespace TheDugout.Data.Configurations.Matches
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    public class PenaltyConfiguration : IEntityTypeConfiguration<Models.Matches.Penalty>
    {
        public void Configure(EntityTypeBuilder<Models.Matches.Penalty> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasOne(p => p.Match)
                .WithMany(m => m.Penalties)
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Team)
                .WithMany()
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.GameSave)
                .WithMany(gs => gs.Penalties)
                .HasForeignKey(m => m.GameSaveId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Player)
                .WithMany()
                .HasForeignKey(p => p.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.Order)
                .IsRequired();

            builder.Property(p => p.IsScored)
                .IsRequired();
        }
    }
}
