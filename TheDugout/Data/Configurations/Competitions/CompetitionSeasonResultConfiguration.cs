namespace TheDugout.Data.Configurations.Competitions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Competitions;
    public class CompetitionSeasonResultConfiguration : IEntityTypeConfiguration<CompetitionSeasonResult>
    {
        public void Configure(EntityTypeBuilder<CompetitionSeasonResult> builder)
        {
            builder.HasKey(csr => csr.Id);

            builder.HasOne(csr => csr.Season)
                   .WithMany(s => s.CompetitionSeasonResults)
                   .HasForeignKey(csr => csr.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(csr => csr.Competition)
                   .WithMany()
                   .HasForeignKey(csr => csr.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(csr => csr.GameSave)
                   .WithMany(gs => gs.CompetitionSeasonResults)
                   .HasForeignKey(csr => csr.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(csr => csr.ChampionTeam)
                   .WithMany()
                   .HasForeignKey(csr => csr.ChampionTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(csr => csr.RunnerUpTeam)
                   .WithMany()
                   .HasForeignKey(csr => csr.RunnerUpTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Awards)
                     .WithOne(a => a.CompetitionSeasonResult)
                     .HasForeignKey(a => a.CompetitionSeasonResultId)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.Property(csr => csr.CompetitionType)
                   .IsRequired();

            builder.Property(csr => csr.Notes)
                   .HasMaxLength(500);
        }
    }

}
