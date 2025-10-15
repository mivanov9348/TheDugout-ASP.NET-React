namespace TheDugout.Data.Configurations.Competitions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Competitions;
    public class CompetitionAwardConfiguration : IEntityTypeConfiguration<CompetitionAward>
    {
        public void Configure(EntityTypeBuilder<CompetitionAward> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.AwardType)
                   .IsRequired();

            builder.Property(a => a.Value)
                   .IsRequired();

            // 🧩 Player (винаги има играч)
            builder.HasOne(a => a.Player)
                   .WithMany()
                   .HasForeignKey(a => a.PlayerId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 🏆 Season (винаги има сезон)
            builder.HasOne(a => a.Season)
                   .WithMany()
                   .HasForeignKey(a => a.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 💾 GameSave (винаги има сейв)
            builder.HasOne(a => a.GameSave)
                   .WithMany(gs => gs.CompetitionAwards)
                   .HasForeignKey(a => a.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ⚽ Competition (може да е null — наградата е свързана с резултата, но може и директно с турнира)
            builder.HasOne(a => a.Competition)
                   .WithMany(c => c.Awards)
                   .HasForeignKey(a => a.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 📊 CompetitionSeasonResult (винаги задължително)
            builder.HasOne(a => a.CompetitionSeasonResult)
                   .WithMany(csr => csr.Awards)
                   .HasForeignKey(a => a.CompetitionSeasonResultId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}