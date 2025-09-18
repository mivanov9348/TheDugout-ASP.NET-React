
using TheDugout.Data;
using TheDugout.Models.Facilities;

namespace TheDugout.Services.Facilities
{
    public class YouthAcademyService : IYouthAcademyService
    {
        private readonly DugoutDbContext _context;
        public YouthAcademyService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task AddYouthAcademyAsync(int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) throw new Exception("Team not found");

            var academy = new YouthAcademy
            {
                TeamId = team.Id,
                Level = 1,
                TalentPointsPerYear = 20
            };

            _context.YouthAcademies.Add(academy);
            await _context.SaveChangesAsync();
        }

            
    }
}
