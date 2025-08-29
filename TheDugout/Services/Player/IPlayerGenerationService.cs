using TheDugout.Models;

namespace TheDugout.Services.Players
{
    public interface IPlayerGenerationService
    {
        /// <summary>
        /// Генерира играчи за даден отбор в конкретен сейв.
        /// </summary>
        /// <param name="save">Записът на играта, към който принадлежи отборът.</param>
        /// <param name="team">Отборът, за който ще се създадат играчи.</param>
        /// <returns>Списък от играчи.</returns>
        List<Models.Player> GenerateTeamPlayers(GameSave save, Models.Team team);
    }
}
