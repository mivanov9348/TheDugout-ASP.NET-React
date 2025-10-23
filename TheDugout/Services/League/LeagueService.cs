namespace TheDugout.Services.League
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Seasons;
    using TheDugout.Services.League.Interfaces;
    using TheDugout.Services.Team.Interfaces;

    public class LeagueService : ILeagueService
    {
        private readonly DugoutDbContext _context;
        private readonly ITeamGenerationService _teamGenerator;

        public LeagueService(DugoutDbContext context, ITeamGenerationService teamGenerator)
        {
            _context = context;
            _teamGenerator = teamGenerator;
        }
        public async Task<List<League>> GenerateLeaguesAsync(GameSave gameSave, Season season)
        {
            var leagues = new List<League>();

            var leagueTemplates = await _context.LeagueTemplates
                .Include(lt => lt.TeamTemplates)
                .AsNoTracking()
                .Where(lt => lt.IsActive)
                .ToDictionaryAsync(lt => lt.Id);

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var lt in leagueTemplates.Values)
            {
                var competition = new Competition
                {
                    Type = CompetitionTypeEnum.League,
                    GameSaveId = gameSave.Id,
                    SeasonId = season.Id
                };

                leagues.Add(new League
                {
                    TemplateId = lt.Id,
                    GameSaveId = gameSave.Id,
                    Season = season,
                    SeasonId = season.Id,
                    CountryId = lt.CountryId,
                    Tier = lt.Tier,
                    TeamsCount = lt.TeamsCount,
                    RelegationSpots = lt.RelegationSpots,
                    PromotionSpots = lt.PromotionSpots,
                    Competition = competition,
                    CompetitionId = competition.Id
                });
            }

            await _context.Leagues.AddRangeAsync(leagues);
            await _context.SaveChangesAsync();

            _context.ChangeTracker.AutoDetectChangesEnabled = true;
            return leagues;
        }

        public async Task GenerateTeamsForLeaguesAsync(GameSave gameSave, List<League> leagues)
        {
            var leagueTemplates = await _context.LeagueTemplates
                .Include(lt => lt.TeamTemplates)
                .AsNoTracking()
                .Where(lt => lt.IsActive)
                .ToDictionaryAsync(lt => lt.Id);

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var league in leagues)
            {
                var lt = leagueTemplates[league.TemplateId];
                var teams = await _teamGenerator.GenerateTeamsAsync(gameSave, league, lt.TeamTemplates);
                league.Teams = teams;
                foreach (var team in teams)
                {
                    gameSave.Teams.Add(team); 
                }
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;
            await _context.SaveChangesAsync();
        }

        public async Task InitializeStandingsAsync(GameSave gameSave,Season season)
        {
            var standings = new List<LeagueStanding>();

            var existingStandings = await _context.LeagueStandings
                .Where(ls => ls.SeasonId == season.Id)
                .Select(ls => new { ls.LeagueId, ls.TeamId })
                .ToListAsync();
            var existingSet = new HashSet<(int, int?)>(existingStandings.Select(x => (x.LeagueId, x.TeamId)));


            var leaguesForThisSeason = await _context.Leagues
                .Include(l => l.Teams) 
                .Where(l => l.SeasonId == season.Id)
                .ToListAsync();

            foreach (var league in leaguesForThisSeason)
            {
                var sortedTeams = league.Teams
                    .OrderByDescending(t => t.Popularity)
                    .ThenBy(t => t.Name)
                    .ToList();

                for (int i = 0; i < sortedTeams.Count; i++)
                {
                    var team = sortedTeams[i];
                    if (!existingSet.Contains((league.Id, team.Id)))
                    {
                        standings.Add(new LeagueStanding
                        {
                            GameSaveId = gameSave.Id,
                            SeasonId = season.Id,
                            LeagueId = league.Id,
                            TeamId = team.Id,
                            Ranking = i + 1
                        });
                    }
                }
            }

            await _context.LeagueStandings.AddRangeAsync(standings);
            await _context.SaveChangesAsync();
        }
        public async Task CopyTeamsFromPreviousSeasonAsync(Season previousSeason, List<League> newSeasonLeagues)
        {
            var previousLeagues = await _context.Leagues
                .Include(l => l.Teams)
                .Where(l => l.SeasonId == previousSeason.Id)
                .ToListAsync();

            // Създаваме lookup по Country + Tier за по-лесно съпоставяне
            var newLeaguesByCountryAndTier = newSeasonLeagues
                .ToDictionary(l => (l.CountryId, l.Tier));

            foreach (var oldLeague in previousLeagues)
            {
                if (!newLeaguesByCountryAndTier.TryGetValue((oldLeague.CountryId, oldLeague.Tier), out var newLeague))
                {
                    continue;
                }

                foreach (var team in oldLeague.Teams)
                {
                    team.LeagueId = newLeague.Id;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task ProcessPromotionsAndRelegationsAsync(GameSave gameSave, Season previousSeason, List<League> newSeasonLeagues)
        {
            // 1. Create Lookup
            var newLeaguesByCountryAndTier = newSeasonLeagues
                .ToDictionary(l => (l.CountryId, l.Tier));

            // 2. Взимаме записите за изпадащи отбори от ПРЕДИШНИЯ сезон
            // (Предполагаме, че CompetitionSeasonResult -> Competition -> SeasonId/LeagueId)
            var relegatedTeamEntries = await _context.CompetitionRelegatedTeams
                .AsNoTracking()
                .Include(rt => rt.CompetitionSeasonResult.Competition.League) // Включваме старата лига
                .Where(rt => rt.GameSaveId == gameSave.Id &&
                             rt.CompetitionSeasonResult.Competition.SeasonId == previousSeason.Id)
                .ToListAsync();

            // 3. Взимаме записите за промотирани отбори от ПРЕДИШНИЯ сезон
            var promotedTeamEntries = await _context.CompetitionPromotedTeams
                .AsNoTracking()
                .Include(pt => pt.CompetitionSeasonResult.Competition.League) // Включваме старата лига
                .Where(pt => pt.GameSaveId == gameSave.Id &&
                             pt.CompetitionSeasonResult.Competition.SeasonId == previousSeason.Id)
                .ToListAsync();

            // 4. Събираме ID-тата на всички отбори, които ще местим
            var relegatedTeamIds = relegatedTeamEntries.Select(rt => rt.TeamId);
            var promotedTeamIds = promotedTeamEntries.Select(pt => pt.TeamId);
            var allTeamIdsToMove = relegatedTeamIds.Concat(promotedTeamIds).Distinct().ToList();

            // 5. Зареждаме самите Team обекти, за да можем да ги ъпдейтнем
            var teamsToUpdate = await _context.Teams
                .Where(t => allTeamIdsToMove.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id);

            if (!teamsToUpdate.Any())
            {
                // Няма нищо за правене
                return;
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            // 6. Обработваме ИЗПАДАЩИТЕ
            foreach (var entry in relegatedTeamEntries)
            {
                var sourceLeague = entry.CompetitionSeasonResult.Competition.League;
                if (sourceLeague == null) continue; // Грешка в данните?

                // Намираме отбора за ъпдейт
                if (teamsToUpdate.TryGetValue(entry.TeamId, out var team))
                {
                    // Намираме новата му лига (Tier + 1)
                    int targetTier = sourceLeague.Tier + 1;
                    if (newLeaguesByCountryAndTier.TryGetValue((sourceLeague.CountryId, targetTier), out var targetLeague))
                    {
                        // Ъпдейтваме ID-то на лигата на отбора!
                        team.LeagueId = targetLeague.Id;
                    }
                    else
                    {
                        // ЛОГ: Грешка, не е намерена лига, в която да изпадне!
                        // (например, вече е в най-долната)
                    }
                }
            }

            // 7. Обработваме ПРОМОТИРАНИТЕ
            foreach (var entry in promotedTeamEntries)
            {
                var sourceLeague = entry.CompetitionSeasonResult.Competition.League;
                if (sourceLeague == null) continue;

                if (teamsToUpdate.TryGetValue(entry.TeamId, out var team))
                {
                    // Намираме новата му лига (Tier - 1)
                    int targetTier = sourceLeague.Tier - 1;
                    if (newLeaguesByCountryAndTier.TryGetValue((sourceLeague.CountryId, targetTier), out var targetLeague))
                    {
                        // Ъпдейтваме ID-то на лигата на отбора!
                        team.LeagueId = targetLeague.Id;
                    }
                    else
                    {
                        // ЛОГ: Грешка, не е намерена лига, в която да се качи!
                        // (например, вече е в най-горната)
                    }
                }
            }

            // 8. Запазваме всички промени по LeagueId на отборите
            await _context.SaveChangesAsync();
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
        public async Task<bool> IsLeagueFinishedAsync(int leagueId)
        {
            bool allMatchesPlayed = !await _context.Fixtures
                .AnyAsync(f => f.LeagueId == leagueId && f.Status != MatchStageEnum.Played);

            if (allMatchesPlayed)
            {
                var league = await _context.Leagues.FirstOrDefaultAsync(l => l.Id == leagueId);
                if (league != null && !league.IsFinished)
                {
                    league.IsFinished = true;
                    await _context.SaveChangesAsync();
                }
            }
            return allMatchesPlayed;
        }
    }
}
