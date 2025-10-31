namespace TheDugout.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Seasons;

    public class SeasonConfiguration : IEntityTypeConfiguration<Season>
    {
        public void Configure(EntityTypeBuilder<Season> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.StartDate).IsRequired();
            builder.Property(e => e.EndDate).IsRequired();
            builder.Property(e => e.CurrentDate).IsRequired();

            builder.HasOne(e => e.GameSave)
                   .WithMany(gs => gs.Seasons)
                   .HasForeignKey(e => e.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Events)
                   .WithOne(ev => ev.Season)
                   .HasForeignKey(ev => ev.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.PlayerSeasonStats)
                   .WithOne(ps => ps.Season)
                   .HasForeignKey(ps => ps.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Fixtures)
                   .WithOne(f => f.Season)
                   .HasForeignKey(f => f.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.EuropeanCups)
                   .WithOne(c => c.Season)
                   .HasForeignKey(c => c.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Competitions)
                   .WithOne(c => c.Season)
                   .HasForeignKey(c => c.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.CompetitionSeasonResults)
                     .WithOne(csr => csr.Season)
                     .HasForeignKey(csr => csr.SeasonId)
                     .OnDelete(DeleteBehavior.Restrict);         
                 
            builder.HasMany(s => s.Leagues)
                   .WithOne(l => l.Season)
                   .HasForeignKey(l => l.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.LeagueStandings)
                   .WithOne(ls => ls.Season)
                   .HasForeignKey(ls => ls.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Cups)
                   .WithOne(c => c.Season)
                   .HasForeignKey(c => c.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.TrainingSessions)
                   .WithOne(ts => ts.Season)
                   .HasForeignKey(ts => ts.SeasonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Awards)
                    .WithOne(a => a.Season)
                    .HasForeignKey(a => a.SeasonId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.PlayerMatchStats)
                    .WithOne(a => a.Season)
                    .HasForeignKey(a => a.SeasonId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.TransferOffers)
                    .WithOne(a => a.Season)
                    .HasForeignKey(a => a.SeasonId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Matches)
                    .WithOne(a => a.Season)
                    .HasForeignKey(a => a.SeasonId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.FinancialTransactions)
                    .WithOne(a => a.Season)
                    .HasForeignKey(a => a.SeasonId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.PlayerTrainings)
                    .WithOne(a => a.Season)
                    .HasForeignKey(a => a.SeasonId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}