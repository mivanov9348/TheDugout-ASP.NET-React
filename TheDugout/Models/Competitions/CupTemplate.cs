using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using TheDugout.Models.Common;

namespace TheDugout.Models.Competitions
{
    public class CupTemplate
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;   
        public string CountryCode { get; set; } = null!;
        public bool IsActive { get; set; } = true;  
  
        public int? MinTeams { get; set; }
        public int? MaxTeams { get; set; }
    }
}
