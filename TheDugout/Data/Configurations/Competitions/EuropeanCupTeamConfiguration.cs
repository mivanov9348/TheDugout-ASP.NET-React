using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models.Competitions;

namespace TheDugout.Data.Configurations
{
    public class EuropeanCupTeamConfiguration : IEntityTypeConfiguration<EuropeanCupTeam>
    {
        public void Configure(EntityTypeBuilder<EuropeanCupTeam> builder)
        {
            builder.ToTable("EuropeanCupTeams");

            builder.HasKey(e => e.Id);


            builder.HasIndex(e => new { e.EuropeanCupId, e.TeamId }).IsUnique();

            builder.HasOne(e => e.EuropeanCup)
                   .WithMany(c => c.Teams)
                   .HasForeignKey(e => e.EuropeanCupId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.GameSave)
                   .WithMany()
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Team)
                   .WithMany(t => t.EuropeanCupTeams)
                   .HasForeignKey(e => e.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
