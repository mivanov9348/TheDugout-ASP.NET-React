namespace TheDugout.Models.Game
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Messages;
    using TheDugout.Models.Players;
    using TheDugout.Models.Seasons;
    using TheDugout.Models.Staff;
    using TheDugout.Models.Teams;
    using TheDugout.Models.Training;
    using TheDugout.Models.Transfers;
    public class GameSave
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = "New Save";

        public int? UserTeamId { get; set; }
        public Team? UserTeam { get; set; }

        public int? BankId { get; set; }
        public Bank Bank { get; set; } = null!;

        public int? CurrentSeasonId { get; set; }
        public Season? CurrentSeason { get; set; } = null!;

        public string NextDayActionLabel { get; set; } = "Next Day →";

        // Leagues
        public ICollection<League> Leagues { get; set; } = new List<League>();
        public ICollection<LeagueStanding> LeagueStandings { get; set; } = new List<LeagueStanding>();

        // Teams, Players, Staff, Seasons
        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<TeamTactic> TeamTactics { get; set; } = new List<TeamTactic>();

        // Messages
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        // Players
        public ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<PlayerMatchStats> PlayerMatchStats { get; set; } = new List<PlayerMatchStats>();
        public ICollection<PlayerSeasonStats> PlayerSeasonStats { get; set; } = new List<PlayerSeasonStats>();
        public ICollection<PlayerAttribute> PlayerAttributes { get; set; } = new List<PlayerAttribute>();
        public ICollection<PlayerCompetitionStats> PlayerCompetitionStats { get; set; } = new List<PlayerCompetitionStats>();
        public ICollection<YouthPlayer> YouthPlayers { get; set; } = new List<YouthPlayer>();
        public ICollection<Shortlist> Shortlist { get; set; } = new List<Shortlist>();

        // Seasons
        public ICollection<Season> Seasons { get; set; } = new List<Season>();
        public ICollection<SeasonEvent> SeasonEvents { get; set; } = new List<SeasonEvent>();

        // Fixtures
        public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();

        // Trainings
        public ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
        public ICollection<PlayerTraining> PlayerTrainings { get; set; } = new List<PlayerTraining>();

        // Competitions
        public ICollection<Competition> Competitions { get; set; } = new List<Competition>();
        public ICollection<CompetitionSeasonResult> CompetitionSeasonResults { get; set; } = new List<CompetitionSeasonResult>();
        public ICollection<CompetitionPromotedTeam> CompetitionPromotedTeams { get; set; } = new List<CompetitionPromotedTeam>();
        public ICollection<CompetitionRelegatedTeam> CompetitionRelegatedTeams { get; set; } = new List<CompetitionRelegatedTeam>();
        public ICollection<CompetitionEuropeanQualifiedTeam> CompetitionEuropeanQualifiedTeams { get; set; } = new List<CompetitionEuropeanQualifiedTeam>();
        public ICollection<CompetitionAward> Awards { get; set; } = new List<CompetitionAward>();

        // Cups
        public ICollection<Cup> Cups { get; set; } = new List<Cup>();
        public ICollection<CupRound> CupRounds { get; set; } = new List<CupRound>();
        public ICollection<CupTeam> CupTeams { get; set; } = new List<CupTeam>();

        // European Cups
        public ICollection<EuropeanCup> EuropeanCups { get; set; } = new List<EuropeanCup>();
        public ICollection<EuropeanCupPhase> EuropeanCupPhases { get; set; } = new List<EuropeanCupPhase>();
        public ICollection<EuropeanCupStanding> EuropeanStandings { get; set; } = new List<EuropeanCupStanding>();
        public ICollection<EuropeanCupTeam> EuropeanCupTeams { get; set; } = new List<EuropeanCupTeam>();

        // Agencies
        public ICollection<Agency> Agencies { get; set; } = new List<Agency>();

        // Facilities
        public ICollection<Stadium> Stadiums { get; set; } = new List<Stadium>();
        public ICollection<YouthAcademy> YouthAcademies { get; set; } = new List<YouthAcademy>();
        public ICollection<TrainingFacility> TrainingFacilities { get; set; } = new List<TrainingFacility>();
      
        // Financial Transactions
        public ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();

        // Matches
        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();

        // Penalties
        public ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();

        // Transfers
        public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
        public ICollection<TransferOffer> TransferOffers { get; set; } = new List<TransferOffer>();

    }
}
