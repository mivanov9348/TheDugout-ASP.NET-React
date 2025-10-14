namespace TheDugout.Data.Configurations.Competitions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Competitions;
    public class CompetitionEuropeanQualifiedTeamConfiguration : IEntityTypeConfiguration<CompetitionEuropeanQualifiedTeam>
    {
        public void Configure(EntityTypeBuilder<CompetitionEuropeanQualifiedTeam> builder)
        {
            builder.HasKey(eq => eq.Id);

            builder.HasOne(eq => eq.CompetitionSeasonResult)
                   .WithMany(csr => csr.EuropeanQualifiedTeams)
                   .HasForeignKey(eq => eq.CompetitionSeasonResultId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(eq => eq.Team)
                   .WithMany()
                   .HasForeignKey(eq => eq.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(eq => eq.GameSave)
                   .WithMany(gs => gs.CompetitionEuropeanQualifiedTeams)
                   .HasForeignKey(eq => eq.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
