namespace TheDugout.Services.Team
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Players;
    using TheDugout.Models.Teams;
    using TheDugout.Services.Facilities;
    using TheDugout.Services.Player.Interfaces;
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
                    Popularity = tt.Popularity,
                    LogoFileName = GenerateLogoFileName(tt.Name)
                };

                var players = _playerGenerator.GenerateTeamPlayers(gameSave, team);
                foreach (var player in players)
                {
                    team.Players.Add(player);
                    gameSave.Players.Add(player);
                }

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

        public async Task UpdatePopularityAsync(Team team, TeamEventType eventType)
        {
            int delta = eventType switch
            {
                TeamEventType.Win => 1,
                TeamEventType.Draw => 0,
                TeamEventType.Loss => -1,
                TeamEventType.TitleWin => 5,
                TeamEventType.CupWin => 3,
                TeamEventType.Promotion => 4,
                TeamEventType.Relegation => -4,
                TeamEventType.EuropeanSuccess => 4,
                TeamEventType.SeasonDecay => -1,
                _ => 0
            };

            team.Popularity = Math.Clamp(team.Popularity + delta, 1, 100);

            _context.Update(team);
            await _context.SaveChangesAsync();
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
            // 🟢 1. Вземи шаблоните на отбори без лига И от неактивни лиги
            var templates = await _context.TeamTemplates
                .Include(tt => tt.League)
                .Where(tt =>
                    tt.LeagueId == null || // отбор без лига
                    (tt.League != null && !tt.League.IsActive)) // или от неактивна лига
                .ToListAsync();

            var teams = new List<Team>();

            foreach (var tt in templates)
            {
                // 🟡 2. Ако отборът идва от неактивна лига — правим го независим (League = null)
                var team = new Team
                {
                    TemplateId = tt.Id,
                    GameSaveId = gameSave.Id,
                    League = null,
                    LeagueId = null,
                    Name = tt.Name,
                    Abbreviation = tt.Abbreviation,
                    CountryId = tt.CountryId,
                    Country = tt.Country,
                    Popularity = tt.Popularity,
                    LogoFileName = GenerateLogoFileName(tt.Name)
                };

                // 🔹 Генерираме играчи
                var players = _playerGenerator.GenerateTeamPlayers(gameSave, team);
                foreach (var player in players)
                {
                    gameSave.Players.Add(player);
                    team.Players.Add(player);
                }

                teams.Add(team);
            }

            // 🟢 3. Записваме отборите
            _context.Teams.AddRange(teams);
            await _context.SaveChangesAsync();

            // 🏟️ 4. Добавяме съоръжения
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
                .Include(t => t.Country)
                .Where(t => t.GameSaveId == gameSaveId)
                .ToListAsync();

            var save = await _context.GameSaves.FindAsync(gameSaveId);
            var rosterPlan = _teamPlanService.GetDefaultRosterPlan();

            foreach (var team in teams)
            {
                // групиране по позиция
                var positionCounts = team.Players
                    .Where(p => p.IsActive && p.Position != null)
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
