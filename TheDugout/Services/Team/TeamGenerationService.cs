namespace TheDugout.Services.Team
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Teams;
    using TheDugout.Services.Facilities;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Models.Players;
    using TheDugout.Services.Team.Interfaces;

    public class TeamGenerationService : ITeamGenerationService
    {
        private readonly IPlayerGenerationService _playerGenerator;
        private readonly IStadiumService _stadiumService;
        private readonly ITrainingFacilitiesService _trainingService;
        private readonly IYouthAcademyService _academyService;
        private readonly ITeamPlanService _teamPlanService;
        private readonly DugoutDbContext _context;

        public TeamGenerationService(IPlayerGenerationService playerGenerator, DugoutDbContext context, IStadiumService stadiumService, ITrainingFacilitiesService trainingFacilitiesService, IYouthAcademyService youthAcademyService, ITeamPlanService teamPlanService)
        {
            _playerGenerator = playerGenerator;
            _context = context;
            _stadiumService = stadiumService;
            _trainingService = trainingFacilitiesService;
            _academyService = youthAcademyService;
            _teamPlanService = teamPlanService;
        }

        public async Task<List<Team>> GenerateTeamsAsync(
    GameSave gameSave,
    League league,
    IEnumerable<TeamTemplate> templates)
        {
            var teams = new List<Team>();

            foreach (var tt in templates)
            {
                var team = new Team
                {
                    TemplateId = tt.Id,
                    GameSaveId = gameSave.Id,
                    League = league,
                    Name = tt.Name,
                    Abbreviation = tt.Abbreviation,
                    CountryId = tt.CountryId,
                    Popularity = 10,
                    LogoFileName = GenerateLogoFileName(tt.Name)
                };

                var players = _playerGenerator.GenerateTeamPlayers(gameSave, team);
                foreach (var player in players)
                {
                    team.Players.Add(player);
                    gameSave.Players.Add(player);
                }

                team.Popularity = CalculateTeamPopularity(team);
                teams.Add(team);
            }

            // Добавяме наведнъж
            _context.Teams.AddRange(teams);

            // Отлагаме създаването на стадион и т.н. до след save
            await _context.SaveChangesAsync();

            // 🔄 Пост-фаза: създаваме съоръжения без SaveChanges вътре
            var stadiums = new List<Stadium>();
            var trainings = new List<TrainingFacility>();
            var academies = new List<YouthAcademy>();

            foreach (var team in teams)
            {
                stadiums.Add(await _stadiumService.AddStadiumAsync(team.Id));
                trainings.Add(await _trainingService.AddTrainingFacilityAsync(team.Id));
                academies.Add(await _academyService.AddYouthAcademyAsync(team.Id));
            }

            await _context.SaveChangesAsync();

            return teams;
        }       

        private int CalculateTeamPopularity(Team team)
        {
            if (team.Players == null || !team.Players.Any())
                return 10;

            var allValues = team.Players
                .SelectMany(p => p.Attributes)
                .Select(pa => pa.Value);

            double avgSkill = allValues.Any() ? allValues.Average() : 10;

            int popularity = 10 + (int)(avgSkill * 2);

            return Math.Clamp(popularity, 1, 100);
        }
        private string GenerateLogoFileName(string teamName)
        {

            var cleanName = System.Text.RegularExpressions.Regex.Replace(
                teamName,
                @"[<>:""/\\|?*]",
                ""
            );

            return $"{cleanName}.png";
        }

        public async Task<List<Team>> GenerateIndependentTeamsAsync(GameSave gameSave)
        {
            var templates = await _context.TeamTemplates
                .Where(tt => tt.LeagueId == null)
                .ToListAsync();

            var teams = new List<Team>();

            foreach (var tt in templates)
            {
                var team = new Team
                {
                    TemplateId = tt.Id,
                    GameSaveId = gameSave.Id,
                    League = null,
                    Name = tt.Name,
                    Abbreviation = tt.Abbreviation,
                    CountryId = tt.CountryId,
                    Country = tt.Country,
                    Popularity = 10,
                    LogoFileName = GenerateLogoFileName(tt.Name)
                };

                // Генерираме играчи
                var players = _playerGenerator.GenerateTeamPlayers(gameSave, team);
                foreach (var player in players)
                {
                    gameSave.Players.Add(player);
                    team.Players.Add(player);
                }

                team.Popularity = CalculateTeamPopularity(team);
                teams.Add(team);
            }

            // Първо записваме отборите, за да получат ID
            _context.Teams.AddRange(teams);
            await _context.SaveChangesAsync();

            // Сега вече имаме team.Id, можем да добавим съоръжения
            foreach (var team in teams)
            {
                await _stadiumService.AddStadiumAsync(team.Id);
                await _trainingService.AddTrainingFacilityAsync(team.Id);
                await _academyService.AddYouthAcademyAsync(team.Id);
            }

            return teams;
        }
        public async Task EnsureTeamRostersAsync(int gameSaveId)
        {
            var teams = await _context.Teams
                .Include(t => t.Players)
                .Where(t => t.GameSaveId == gameSaveId)
                .ToListAsync();

            var save = await _context.GameSaves.FindAsync(gameSaveId);
            var rosterPlan = _teamPlanService.GetDefaultRosterPlan();

            foreach (var team in teams)
            {
                // групиране по позиция
                var positionCounts = team.Players
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.Position.Code)
                    .ToDictionary(g => g.Key, g => g.Count());

                var newPlayersNeeded = new List<Player>();

                foreach (var kv in rosterPlan)
                {
                    string positionCode = kv.Key;
                    int requiredCount = kv.Value;

                    if (positionCode == "ANY") continue; // ANY е за допълнителни играчи

                    positionCounts.TryGetValue(positionCode, out int currentCount);
                    int missing = requiredCount - currentCount;

                    if (missing > 0)
                    {
                        for (int i = 0; i < missing; i++)
                        {
                            var newPlayer = _playerGenerator.CreateBasePlayer(save, team, team.Country,
                                _context.Positions.First(p => p.Code == positionCode));
                            newPlayersNeeded.Add(newPlayer);
                        }
                    }
                }

                if (newPlayersNeeded.Any())
                {
                    _context.Players.AddRange(newPlayersNeeded);
                }
            }

            await _context.SaveChangesAsync();
        }


    }
}
