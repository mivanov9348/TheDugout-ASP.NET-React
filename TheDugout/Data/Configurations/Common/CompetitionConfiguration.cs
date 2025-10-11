namespace TheDugout.Data.Configurations.Common
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Controllers;
    using TheDugout.Models.Common;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Leagues;

    public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
    {
        public void Configure(EntityTypeBuilder<Competition> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Type)
                   .IsRequired();

            builder.HasOne(c => c.Season)
                   .WithMany(s => s.Competitions)
                   .HasForeignKey(c => c.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Matches)
                   .WithOne(m => m.Competition)
                   .HasForeignKey(m => m.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.PlayerStats)
                   .WithOne(ps => ps.Competition)
                   .HasForeignKey(ps => ps.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.League)
                   .WithOne(l => l.Competition)
                   .HasForeignKey<League>(c => c.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Cup)
                   .WithOne(cu => cu.Competition)
                   .HasForeignKey<Cup>(c => c.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.EuropeanCup)
                   .WithOne(ec => ec.Competition)
                   .HasForeignKey<EuropeanCup>(c => c.CompetitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.GameSave)
                     .WithMany(gs => gs.Competitions)
                     .HasForeignKey(c => c.GameSaveId)
                     .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
