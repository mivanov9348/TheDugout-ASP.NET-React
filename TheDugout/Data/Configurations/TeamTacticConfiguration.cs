using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

namespace TheDugout.Data.Configurations
{
    public class TeamTacticConfiguration : IEntityTypeConfiguration<TeamTactic>
    {
        public void Configure(EntityTypeBuilder<TeamTactic> builder)
        {
            builder.ToTable("TeamTactics");

            builder.HasKey(tt => tt.Id);

            builder.Property(tt => tt.CustomName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(tt => tt.Team)
                   .WithOne(t => t.TeamTactic)
                   .HasForeignKey<TeamTactic>(tt => tt.TeamId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tt => tt.Tactic)
                   .WithMany(t => t.TeamTactics)
                   .HasForeignKey(tt => tt.TacticId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
