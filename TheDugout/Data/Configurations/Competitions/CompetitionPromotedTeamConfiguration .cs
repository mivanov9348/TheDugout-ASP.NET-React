namespace TheDugout.Data.Configurations.Competitions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Competitions;
    public class CompetitionPromotedTeamConfiguration : IEntityTypeConfiguration<CompetitionPromotedTeam>
    {
        public void Configure(EntityTypeBuilder<CompetitionPromotedTeam> builder)
        {
            builder.HasKey(pt => pt.Id);

            builder.HasOne(pt => pt.CompetitionSeasonResult)
                   .WithMany(csr => csr.PromotedTeams)
                   .HasForeignKey(pt => pt.CompetitionSeasonResultId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pt => pt.Team)
                   .WithMany()
                   .HasForeignKey(pt => pt.TeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pt => pt.GameSave)
                   .WithMany(gs => gs.CompetitionPromotedTeams)
                   .HasForeignKey(pt => pt.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
