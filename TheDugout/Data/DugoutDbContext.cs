namespace TheDugout.Data
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Models.Common;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Messages;
    using TheDugout.Models.Players;
    using TheDugout.Models.Seasons;
    using TheDugout.Models.Staff;
    using TheDugout.Models.Teams;
    using TheDugout.Models.Training;
    using TheDugout.Models.Transfers;

    public class DugoutDbContext : DbContext
    {
        public DugoutDbContext(DbContextOptions<DugoutDbContext> options) : base(options)
        {
        }

        public DbSet<Bank> Banks { get; set; }
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<FirstName> FirstNames { get; set; }
        public DbSet<LastName> LastNames { get; set; }
        public DbSet<GameSetting> GameSettings { get; set; }
        public DbSet<LeagueTemplate> LeagueTemplates { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<CupTemplate> CupTemplates { get; set; }
        public DbSet<Cup> Cups { get; set; }
        public DbSet<CupRound> CupRounds { get; set; }
        public DbSet<CupTeam> CupTeams { get; set; }
        public DbSet<TeamTemplate> TeamTemplates { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<LeagueStanding> LeagueStandings { get; set; }
        public DbSet<GameSave> GameSaves { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<SeasonEvent> SeasonEvents { get; set; }
        public DbSet<PlayerMatchStats> PlayerMatchStats { get; set; }
        public DbSet<PlayerSeasonStats> PlayerSeasonStats { get; set; }
        public DbSet<PlayerAttribute> PlayerAttributes { get; set; }
        public DbSet<AttributeDefinition> Attributes { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<PositionWeight> PositionWeights { get; set; }  
        public DbSet<Fixture> Fixtures { get; set; }
        public DbSet<Tactic> Tactics { get; set; }
        public DbSet<TeamTactic> TeamTactics { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<PlayerTraining> PlayerTrainings { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<TransferOffer> TransferOffers { get; set; }
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<CompetitionSeasonResult> CompetitionSeasonResults { get; set; }
        public DbSet<CompetitionRelegatedTeam> CompetitionRelegatedTeams { get; set; }
        public DbSet<CompetitionPromotedTeam> CompetitionPromotedTeams { get; set; }
        public DbSet<CompetitionEuropeanQualifiedTeam> CompetitionEuropeanQualifiedTeams { get; set; }
        public DbSet<CompetitionAward> CompetitionAwards { get; set; }
        public DbSet<EuropeanCup> EuropeanCups { get; set; }
        public DbSet<EuropeanCupPhase> EuropeanCupPhases { get; set; }
        public DbSet<EuropeanCupTeam> EuropeanCupTeams { get; set; }
        public DbSet<EuropeanCupPhaseTemplate> EuropeanCupPhaseTemplates { get; set; }
        public DbSet<EuropeanCupStanding> EuropeanCupStandings { get; set; }
        public DbSet<EuropeanCupTemplate> EuropeanCupTemplates { get; set; }
        public DbSet<AgencyTemplate> AgencyTemplates { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Stadium> Stadiums { get; set; }
        public DbSet<TrainingFacility> TrainingFacilities { get; set; }
        public DbSet<YouthAcademy> YouthAcademies { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchEvent> MatchEvents { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<EventOutcome> EventOutcomes { get; set; }
        public DbSet<CommentaryTemplate> CommentaryTemplates { get; set; }
        public DbSet<EventAttributeWeight> EventAttributeWeights { get; set; }
        public DbSet<Penalty> Penalties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DugoutDbContext).Assembly);
        }
    }
}
