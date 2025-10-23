namespace TheDugout.Controllers
{

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Seasons;

    [ApiController]
    [Route("api/calendar")]
    public class CalendarController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public CalendarController(DugoutDbContext context)
        {
            _context = context;
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value
                              ?? User.FindFirst("id")?.Value;

            if (int.TryParse(userIdClaim, out var parsed)) return parsed;
            return null;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCalendar([FromQuery] int gameSaveId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            // ✅ Взимаме активния сезон за този GameSave
            var activeSeason = await _context.Seasons
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

            if (activeSeason == null)
                return NotFound($"No active season found for GameSave {gameSaveId}");

            // ✅ Използваме ID-то на активния сезон надолу
            var season = await _context.Seasons
                .AsNoTracking()
                .Where(s => s.Id == activeSeason.Id && s.GameSave.UserId == userId)
                .Select(s => new
                {
                    seasonId = s.Id,
                    startDate = s.StartDate.Date,
                    endDate = s.EndDate.Date,
                    currentDate = s.CurrentDate.Date,
                    userTeamId = s.GameSave.UserTeamId,
                    fixtures = s.Fixtures
                        .Where(f => f.HomeTeamId == s.GameSave.UserTeamId || f.AwayTeamId == s.GameSave.UserTeamId)
                        .Select(f => new
                        {
                            f.Id,
                            date = f.Date.Date,
                            f.CompetitionType,
                            HomeTeam = f.HomeTeam.Name,
                            AwayTeam = f.AwayTeam.Name,
                            f.HomeTeamId,
                            f.AwayTeamId
                        }),
                    seasonEvents = s.Events
                        .Select(e => new
                        {
                            e.Id,
                            date = e.Date.Date,
                            e.Type,
                            e.Description,
                            e.IsOccupied
                        })
                })
                .FirstOrDefaultAsync();

            if (season == null)
                return NotFound("Season not found or not accessible by this user.");

            if (season.userTeamId == null)
                return BadRequest("User team not set for this save.");

            // ⚽ Мачове
            var fixtureEvents = season.fixtures.Select(f =>
            {
                bool isHome = f.HomeTeamId == season.userTeamId;
                string opponent = isHome ? f.AwayTeam : f.HomeTeam;
                string ha = isHome ? "(H)" : "(A)";

                string competition = f.CompetitionType.ToString();
                string description = $"{competition}, {opponent} {ha}";

                SeasonEventType type = f.CompetitionType switch
                {
                    CompetitionTypeEnum.League => SeasonEventType.ChampionshipMatch,
                    CompetitionTypeEnum.DomesticCup => SeasonEventType.CupMatch,
                    CompetitionTypeEnum.EuropeanCup => SeasonEventType.EuropeanMatch,
                    _ => SeasonEventType.Other
                };

                return new
                {
                    id = f.Id,
                    date = f.date.ToString("yyyy-MM-dd"),
                    type = type.ToString(),
                    description,
                    isOccupied = true
                };
            });

            // 📅 Други събития
            var otherEvents = season.seasonEvents.Select(e => new
            {
                id = e.Id,
                date = e.date.ToString("yyyy-MM-dd"),
                type = e.Type.ToString(),
                description = string.IsNullOrWhiteSpace(e.Description) ? e.Type.ToString() : e.Description,
                isOccupied = e.IsOccupied
            });

            // 📌 Всички събития
            var allEvents = fixtureEvents.Concat(otherEvents).ToList();

            // 🎯 Свободни дни
            var allDates = Enumerable.Range(0, (season.endDate - season.startDate).Days + 1)
                .Select(offset => season.startDate.AddDays(offset).ToString("yyyy-MM-dd"));

            var occupiedDates = allEvents.Select(e => e.date).ToHashSet();

            var freeDayEvents = allDates
                .Where(d => !occupiedDates.Contains(d))
                .Select(d => new
                {
                    id = 0,
                    date = d,
                    type = SeasonEventType.TrainingDay.ToString(),
                    description = "Training",
                    isOccupied = false
                });

            var result = new
            {
                season.seasonId,
                startDate = season.startDate.ToString("yyyy-MM-dd"),
                endDate = season.endDate.ToString("yyyy-MM-dd"),
                currentDate = season.currentDate.ToString("yyyy-MM-dd"),
                events = allEvents.Concat(freeDayEvents).OrderBy(e => e.date)
            };

            return Ok(result);
        }
    }
}