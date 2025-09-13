using TheDugout.Models.Common;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;
using TheDugout.Models.Seasons;
using TheDugout.Models.Teams;

namespace TheDugout.Models.Competitions
    {
        public class League
        {
            public int Id { get; set; }

            public int TemplateId { get; set; }
            public LeagueTemplate Template { get; set; } = null!;

            public int GameSaveId { get; set; }
            public GameSave GameSave { get; set; } = null!;

            public int SeasonId { get; set; }
            public Season Season { get; set; } = null!;

            public int CountryId { get; set; }
            public Country Country { get; set; } = null!;

            public int Tier { get; set; }
            public int TeamsCount { get; set; }
            public int RelegationSpots { get; set; }
            public int PromotionSpots { get; set; }

            public ICollection<Team> Teams { get; set; } = new List<Team>();
            public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
            public ICollection<LeagueStanding> Standings { get; set; } = new List<LeagueStanding>();
        }
    }
