using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Cups;

namespace TheDugout.Data.Configurations
{
    public class CupTeamConfiguration : IEntityTypeConfiguration<CupTeam>
    {
        public void Configure(EntityTypeBuilder<CupTeam> builder)
        {
            builder.HasKey(ct => ct.Id);

            builder.HasOne(ct => ct.Cup)
                .WithMany(c => c.Teams)
                .HasForeignKey(ct => ct.CupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ct => ct.Team)
                .WithMany(t => t.CupTeams)
                .HasForeignKey(ct => ct.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.GameSave)
                .WithMany() 
                .HasForeignKey(l => l.GameSaveId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
