using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TheDugout.Data.Configurations.Matches
{
    public class PenaltyConfiguration : IEntityTypeConfiguration<Models.Matches.Penalty>
    {
        public void Configure(EntityTypeBuilder<Models.Matches.Penalty> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasOne(p => p.Match)
                .WithMany(m => m.Penalties)
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Team)
                .WithMany()
                .HasForeignKey(p => p.TeamId)
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
