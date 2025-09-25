using TheDugout.Models.Common;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;
using TheDugout.Models.Staff;
using TheDugout.Models.Teams;

namespace TheDugout.Models.Players
{
    public class Player
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
        public int? TeamId { get; set; }
        public Team? Team { get; set; }
        public int? AgencyId { get; set; }
        public Agency? Agency { get; set; }
        public int? CountryId { get; set; }
        public Country? Country { get; set; }
        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;
        public int KitNumber { get; set; }
        public double HeightCm { get; set; }
        public double WeightKg { get; set; }
        public bool IsActive { get; set; }

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public decimal Price { get; set; }
        public string AvatarFileName { get; set; } = null!;

        public int CurrentAbility
        {
            get;set;
        }

        public int PotentialAbility { get; set; }

        public ICollection<PlayerAttribute> Attributes { get; set; } = new List<PlayerAttribute>();
        public ICollection<PlayerMatchStats> MatchStats { get; set; } = new List<PlayerMatchStats>();
        public ICollection<PlayerSeasonStats> SeasonStats { get; set; } = new List<PlayerSeasonStats>();
        public ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();

    }
}
