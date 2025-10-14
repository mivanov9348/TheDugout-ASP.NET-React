namespace TheDugout.Data.Configurations.Competitions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Competitions;
    public class CompetitionRelegatedTeamConfiguration : IEntityTypeConfiguration<CompetitionRelegatedTeam>
    {
        public void Configure(EntityTypeBuilder<CompetitionRelegatedTeam> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.HasOne(rt => rt.CompetitionSeasonResult)
                   .WithMany(csr => csr.RelegatedTeams)
                   .HasForeignKey(rt => rt.CompetitionSeasonResultId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rt => rt.Team)
                   .WithMany()
                   .HasForeignKey(rt => rt.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(rt => rt.GameSave)
                   .WithMany(gs => gs.CompetitionRelegatedTeams)
                   .HasForeignKey(rt => rt.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
