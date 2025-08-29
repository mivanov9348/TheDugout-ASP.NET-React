namespace TheDugout.Services.Team
{
    public class TeamPlanService : ITeamPlanService
    {
        public Dictionary<string, int> GetDefaultRosterPlan()
        {
            return new Dictionary<string, int>
            {
                { "GK", 1 },
                { "DEF", 4 },
                { "MID", 4 },
                { "ATT", 2 },
                { "ANY", 5 } 
            };
        }
    }
}
