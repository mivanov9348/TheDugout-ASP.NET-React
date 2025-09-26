﻿using TheDugout.Models.Common;
using TheDugout.Models.Competitions;
using TheDugout.Models.Facilities;
using TheDugout.Models.Finance;
using TheDugout.Models.Fixtures;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;
using TheDugout.Models.Players;
using TheDugout.Models.Training;

namespace TheDugout.Models.Teams
{
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
        public int Popularity { get; set; } = 10;

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

        public TeamTactic? TeamTactic { get; set; }
    }
}
