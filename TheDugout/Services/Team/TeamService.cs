using AutoMapper;
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
    private readonly IMapper _mapper;

    public TeamService(DugoutDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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
            Players = _mapper.Map<List<PlayerDto>>(user.CurrentSave.UserTeam.Players)
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
            Players = _mapper.Map<List<PlayerDto>>(save.UserTeam.Players)
        };
    }
}
