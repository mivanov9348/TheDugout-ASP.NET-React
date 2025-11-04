namespace TheDugout.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Seasons;
    public class GameSaveConfiguration : IEntityTypeConfiguration<GameSave>
    {
        public void Configure(EntityTypeBuilder<GameSave> builder)
        {
            builder.ToTable("GameSaves");
            builder.HasKey(e => e.Id);            

            // Users
            builder.HasOne(gs => gs.User)
                   .WithMany(u => u.GameSaves)
                   .HasForeignKey(gs => gs.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.UserTeam)
                   .WithMany()
                   .HasForeignKey(e => e.UserTeamId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Messages
            builder.HasMany(e => e.Messages)
                   .WithOne(m => m.GameSave)
                   .HasForeignKey(m => m.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Seasons
            builder.HasOne(gs => gs.CurrentSeason)
                   .WithMany()
                   .HasForeignKey(gs => gs.CurrentSeasonId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false);

            builder.HasMany(e => e.Seasons)
                   .WithOne(s => s.GameSave)
                   .HasForeignKey(s => s.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.SeasonEvents)
                     .WithOne(se => se.GameSave)
                     .HasForeignKey(se => se.GameSaveId)
                     .OnDelete(DeleteBehavior.Restrict);

            // Fixture
            builder.HasMany(gs => gs.Fixtures)
                   .WithOne(f => f.GameSave)
                   .HasForeignKey(f => f.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Competition
            builder.HasMany(gs => gs.Competitions)
                   .WithOne(c => c.GameSave)
                   .HasForeignKey(c => c.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs=>gs.CompetitionSeasonResults)
                   .WithOne(csr => csr.GameSave)
                   .HasForeignKey(csr => csr.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs=>gs.CompetitionPromotedTeams)
                   .WithOne(cpt => cpt.GameSave)
                   .HasForeignKey(cpt => cpt.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs=>gs.CompetitionRelegatedTeams)
                   .WithOne(crt => crt.GameSave)
                   .HasForeignKey(crt => crt.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs=>gs.CompetitionEuropeanQualifiedTeams)
                   .WithOne(cqt => cqt.GameSave)
                   .HasForeignKey(cqt => cqt.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Awards)
                    .WithOne(a => a.GameSave)
                    .HasForeignKey(a => a.GameSaveId)
                    .OnDelete(DeleteBehavior.Restrict);

            // League
            builder.HasMany(gs => gs.Leagues)
                   .WithOne(l => l.GameSave)
                   .HasForeignKey(l => l.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.LeagueStandings)
                   .WithOne(ls => ls.GameSave)
                   .HasForeignKey(ls => ls.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Cup
            builder.HasMany(gs => gs.Cups)
                   .WithOne(l => l.GameSave)
                   .HasForeignKey(l => l.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.CupRounds)
                   .WithOne(l => l.GameSave)
                   .HasForeignKey(l => l.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.CupTeams)
                   .WithOne(l => l.GameSave)
                   .HasForeignKey(l => l.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            // European Cups
            builder.HasMany(gs => gs.EuropeanCups)
                   .WithOne(ec => ec.GameSave)
                   .HasForeignKey(ec => ec.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.EuropeanCupPhases)
                    .WithOne(ecp => ecp.GameSave)
                    .HasForeignKey(ecp => ecp.GameSaveId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.EuropeanStandings)
                    .WithOne(ecs => ecs.GameSave)
                    .HasForeignKey(ecs => ecs.GameSaveId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.EuropeanCupTeams)
                    .WithOne(ect => ect.GameSave)
                    .HasForeignKey(ect => ect.GameSaveId)
                    .OnDelete(DeleteBehavior.Restrict);

            // Facilities
            builder.HasMany(gs => gs.Stadiums)
                    .WithOne(s => s.GameSave)
                    .HasForeignKey(s => s.GameSaveId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.TrainingFacilities)
                   .WithOne(tf => tf.GameSave)
                   .HasForeignKey(tf => tf.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.YouthAcademies)
                    .WithOne(ya => ya.GameSave)
                    .HasForeignKey(ya => ya.GameSaveId)
                    .OnDelete(DeleteBehavior.Restrict);

            // Match
            builder.HasMany(gs => gs.Matches)
                   .WithOne(m => m.GameSave)
                   .HasForeignKey(m => m.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.MatchEvents)
                   .WithOne(me => me.GameSave)
                   .HasForeignKey(me => me.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Penalty
            builder.HasMany(gs => gs.Penalties)
                   .WithOne(p => p.GameSave)
                   .HasForeignKey(p => p.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);


            // Players
            builder.HasMany(gs => gs.Players)
                   .WithOne(p => p.GameSave)
                   .HasForeignKey(p => p.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.PlayerMatchStats)
                   .WithOne(pms => pms.GameSave)
                   .HasForeignKey(pms => pms.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.PlayerSeasonStats)
                     .WithOne(pss => pss.GameSave)
                     .HasForeignKey(pss => pss.GameSaveId)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.PlayerAttributes)
                     .WithOne(pa => pa.GameSave)
                     .HasForeignKey(pa => pa.GameSaveId)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.YouthPlayers)
                     .WithOne(pa => pa.GameSave)
                     .HasForeignKey(pa => pa.GameSaveId)
                     .OnDelete(DeleteBehavior.Restrict);

            // Teams
            builder.HasMany(gs => gs.Teams)
                   .WithOne(t => t.GameSave)
                   .HasForeignKey(t => t.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.TeamTactics)
                     .WithOne(tt => tt.GameSave)
                     .HasForeignKey(tt => tt.GameSaveId)
                     .OnDelete(DeleteBehavior.Restrict);

            // Bank
            builder.HasOne(gs => gs.Bank)
                    .WithOne(b => b.GameSave)
                    .HasForeignKey<Bank>(b => b.GameSaveId)
                    .OnDelete(DeleteBehavior.Restrict);


            // Financial Transactions
            builder.HasMany(gs => gs.FinancialTransactions)
                   .WithOne(ft => ft.GameSave)
                   .HasForeignKey(ft => ft.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Agency
            builder.HasMany(gs => gs.Agencies)
                     .WithOne(a => a.GameSave)
                     .HasForeignKey(a => a.GameSaveId)
                     .OnDelete(DeleteBehavior.Restrict);

            // Training
            builder.HasMany(gs => gs.TrainingSessions)
                   .WithOne(ts => ts.GameSave)
                   .HasForeignKey(ts => ts.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.PlayerTrainings)
                   .WithOne(pt => pt.GameSave)
                   .HasForeignKey(pt => pt.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Transfer
            builder.HasMany(gs => gs.Transfers)
                   .WithOne(t => t.GameSave)
                   .HasForeignKey(t => t.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(gs => gs.TransferOffers)
                   .WithOne(to => to.GameSave)
                   .HasForeignKey(to => to.GameSaveId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}