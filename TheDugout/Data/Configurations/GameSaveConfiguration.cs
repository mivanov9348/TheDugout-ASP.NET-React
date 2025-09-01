using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class GameSaveConfiguration : IEntityTypeConfiguration<GameSave>
    {
        public void Configure(EntityTypeBuilder<GameSave> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(gs => gs.User)
                  .WithMany(u => u.GameSaves)
                  .HasForeignKey(gs => gs.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.UserTeam)
                   .WithMany()
                   .HasForeignKey(e => e.UserTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Messages)
                   .WithOne(m => m.GameSave)
                   .HasForeignKey(m => m.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Seasons)
                   .WithOne(s => s.GameSave)
                   .HasForeignKey(s => s.GameSaveId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(gs => gs.Fixtures)
       .WithOne(f => f.GameSave)
       .HasForeignKey(f => f.GameSaveId)
       .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
