using TheDugout.Models.Enums;
using TheDugout.Models.Game;
using TheDugout.Models.Seasons;

namespace TheDugout.Models.Common
{
    public class Competition
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public CompetitionTypeEnum Type { get; set; }

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
    }

}
