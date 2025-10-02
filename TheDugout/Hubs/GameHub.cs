using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Services.Game;

namespace TheDugout.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly DugoutDbContext _context;
    private readonly IGameDayService _gameDayService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GameHub(
        DugoutDbContext context,
        IGameDayService gameDayService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _gameDayService = gameDayService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var guidUserId))
        {
            var gameStatus = await GetGameStatusForUserAsync(guidUserId);
            if (gameStatus != null)
            {
                await Clients.Caller.SendAsync("GameUpdated", gameStatus);
            }
        }

        await base.OnConnectedAsync();
    }

    public async Task SendLog(string message)
    {
        await Clients.Caller.SendAsync("ReceiveLog", message);
    }      

    private async Task<object> GetGameStatusForUserAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.CurrentSave)
                .ThenInclude(s => s.Seasons)
            .Include(u => u.CurrentSave)
                .ThenInclude(s => s.UserTeam)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.CurrentSave == null)
            return null;

        var save = user.CurrentSave;
        var season = save.Seasons?.FirstOrDefault();

        // Проверка за активен мач и мачове днес — това трябва да го имплементираш
        // За сега ще използвам примерни стойности
        var hasMatchesToday = season?.CurrentDate.Date == DateTime.UtcNow.Date;
        var hasUnplayedMatchesToday = hasMatchesToday; // упростено
        var activeMatch = (object)null; // можеш да добавиш логика за това

        return new
        {
            gameSave = new
            {
                id = save.Id,
                userTeam = new
                {
                    name = save.UserTeam?.Name ?? "Unknown",
                    leagueName = save.UserTeam?.League?.Template.Name ?? "Unknown League",
                    balance = save.UserTeam?.Balance ?? 0
                },
                seasons = season != null ? new[]
                {
                    new { currentDate = season.CurrentDate }
                } : Array.Empty<object>()
            },
            hasMatchesToday,
            hasUnplayedMatchesToday,
            activeMatch
        };
    }
}