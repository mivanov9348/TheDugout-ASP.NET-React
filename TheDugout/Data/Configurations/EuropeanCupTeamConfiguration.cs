using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDugout.Models;

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
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Team)
                   .WithMany(t => t.EuropeanCupTeams)
                   .HasForeignKey(e => e.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
