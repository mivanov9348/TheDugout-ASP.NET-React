using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(e => e.Team)
                   .WithMany(t => t.Players)
                   .HasForeignKey(e => e.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Country)
                   .WithMany(c => c.Players)
                   .HasForeignKey(e => e.CountryId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.GameSave)
                   .WithMany(gs => gs.Players)
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Attributes)
                   .WithOne(a => a.Player)
                   .HasForeignKey<PlayerAttributes>(a => a.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.MatchStats)
                   .WithOne(ms => ms.Player)
                   .HasForeignKey(ms => ms.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.SeasonStats)
                   .WithOne(ss => ss.Player)
                   .HasForeignKey(ss => ss.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Position)
                   .WithMany(p => p.Players)
                   .HasForeignKey(e => e.PositionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
