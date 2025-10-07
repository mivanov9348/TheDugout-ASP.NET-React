using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.DTOs.Player;
using TheDugout.DTOs.Team;

public interface ITeamService
{
    Task<TeamDto?> GetMyTeamAsync(int userId);
    Task<TeamDto?> GetTeamBySaveAsync(int saveId);
}

public class TeamService : ITeamService
{
    private readonly DugoutDbContext _context;

    public TeamService(DugoutDbContext context)
    {
        _context = context;
    }

    public async Task<TeamDto?> GetMyTeamAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.CurrentSave)
                .ThenInclude(gs => gs.UserTeam)
                    .ThenInclude(t => t.Players)
                        .ThenInclude(p => p.Position)
            .Include(u => u.CurrentSave.UserTeam.Players)
                .ThenInclude(p => p.Country)
            .Include(u => u.CurrentSave.UserTeam.Players)
                .ThenInclude(p => p.Attributes).ThenInclude(a => a.Attribute)
            .Include(u => u.CurrentSave.UserTeam.Players)
                .ThenInclude(p => p.SeasonStats)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.CurrentSave?.UserTeam == null) return null;

        return new TeamDto
        {
            TeamId = user.CurrentSave.UserTeam.Id,
            TeamName = user.CurrentSave.UserTeam.Name,
            Players = user.CurrentSave.UserTeam.Players.Select(p => new PlayerDto
            {
                Id = p.Id,
                FullName = p.FirstName + " " + p.LastName,
                Position = p.Position.Name,
                PositionId = p.PositionId,
                KitNumber = p.KitNumber,
                Age = p.Age,
                Country = p.Country != null ? p.Country.Name : "",
                HeightCm = p.HeightCm,
                WeightKg = p.WeightKg,
                Price = p.Price,
                TeamName = user.CurrentSave.UserTeam.Name,
                AvatarFileName = p.AvatarFileName,
                Attributes = p.Attributes.Select(a => new PlayerAttributeDto
                {
                    AttributeId = a.AttributeId,
                    Name = a.Attribute.Name,
                    Value = a.Value
                }).ToList(),
                SeasonStats = p.SeasonStats.Select(s => new PlayerSeasonStatsDto
                {
                    SeasonId = s.SeasonId,
                    MatchesPlayed = s.MatchesPlayed,
                    Goals = s.Goals
                }).ToList()
            }).ToList()
        };
    }

    public async Task<TeamDto?> GetTeamBySaveAsync(int saveId)
    {
        var save = await _context.GameSaves
            .Include(gs => gs.UserTeam)
                .ThenInclude(t => t.Players)
                    .ThenInclude(p => p.Position)
            .Include(gs => gs.UserTeam.Players)
                .ThenInclude(p => p.Country)
            .Include(gs => gs.UserTeam.Players)
                .ThenInclude(p => p.Attributes).ThenInclude(a => a.Attribute)
            .Include(gs => gs.UserTeam.Players)
                .ThenInclude(p => p.SeasonStats)
            .FirstOrDefaultAsync(gs => gs.Id == saveId);

        if (save?.UserTeam == null) return null;

        return new TeamDto
        {
            TeamId = save.UserTeam.Id,
            TeamName = save.UserTeam.Name,
            Players = save.UserTeam.Players.Select(p => new PlayerDto
            {
                Id = p.Id,
                FullName = p.FirstName + " " + p.LastName,
                Position = p.Position.Name,
                PositionId = p.PositionId,
                KitNumber = p.KitNumber,
                Age = p.Age,
                Country = p.Country != null ? p.Country.Name : "",
                HeightCm = p.HeightCm,
                WeightKg = p.WeightKg,
                Price = p.Price,
                TeamName = save.UserTeam.Name,
                AvatarFileName = p.AvatarFileName,
                Attributes = p.Attributes.Select(a => new PlayerAttributeDto
                {
                    AttributeId = a.AttributeId,
                    Name = a.Attribute.Name,
                    Value = a.Value
                }).ToList(),
                SeasonStats = p.SeasonStats.Select(s => new PlayerSeasonStatsDto
                {
                    SeasonId = s.SeasonId,
                    MatchesPlayed = s.MatchesPlayed,
                    Goals = s.Goals
                }).ToList()
            }).ToList()
        };
    }
}
