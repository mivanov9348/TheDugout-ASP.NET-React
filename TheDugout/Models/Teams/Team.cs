namespace TheDugout.Models.Teams
{
    using System.ComponentModel.DataAnnotations.Schema;
    using TheDugout.Models.Common;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Players;
    using TheDugout.Models.Training;
    using TheDugout.Models.Transfers;
    public class Team
    {
        public int Id { get; set; }

        public int TemplateId { get; set; }
        public TeamTemplate Template { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public string LogoFileName { get; set; } = "default_logo.png";

        public int? LeagueId { get; set; }
        public League? League { get; set; }

        public int? CountryId { get; set; }
        public Country? Country { get; set; } = null!;

        public Stadium? Stadium { get; set; }
        public TrainingFacility? TrainingFacility { get; set; }
        public YouthAcademy? YouthAcademy { get; set; }

        public decimal Balance { get; set; }
        public double PopularityValue { get; set; } = 10; 
        [NotMapped]
        public int Popularity => (int)Math.Round(PopularityValue);

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<Fixture> HomeFixtures { get; set; } = new List<Fixture>();
        public ICollection<Fixture> AwayFixtures { get; set; } = new List<Fixture>();

        public ICollection<EuropeanCupTeam> EuropeanCupTeams { get; set; } = new List<EuropeanCupTeam>();
        public ICollection<LeagueStanding> LeagueStandings { get; set; } = new List<LeagueStanding>();
        public ICollection<EuropeanCupStanding> EuropeanCupStandings { get; set; } = new List<EuropeanCupStanding>();

        public ICollection<CupTeam> CupTeams { get; set; } = new List<CupTeam>();

        public ICollection<FinancialTransaction> TransactionsFrom { get; set; } = new List<FinancialTransaction>();
        public ICollection<FinancialTransaction> TransactionsTo { get; set; } = new List<FinancialTransaction>();

        public ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();

        public ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();
        public ICollection<TransferOffer> SentTransferOffers { get; set; } = new List<TransferOffer>();
        public ICollection<TransferOffer> ReceivedTransferOffers { get; set; } = new List<TransferOffer>();

        public TeamTactic? TeamTactic { get; set; }
    }
}
