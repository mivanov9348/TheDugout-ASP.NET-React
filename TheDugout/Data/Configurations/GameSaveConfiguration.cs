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

            builder.HasOne(e => e.User)
                   .WithMany(u => u.GameSaves)
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

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
        }
    }
}
