using Microsoft.EntityFrameworkCore;

namespace TheDugout.Services.Game
{
    public class GameDayService : IGameDayService
    {
        public GameDayService()
        {
        }

        public async void ProcessNextDayAsync(int saveId)
        {
            //// 1. Зареждаме сейва
            //var save = await _context.GameSaves
            //    .Include(gs => gs.Seasons)
            //    .FirstOrDefaultAsync(gs => gs.Id == saveId);

            //if (save == null)
            //    throw new InvalidOperationException("Game save not found");

            //// 2. Смяна на дата
            //save.CurrentDate = save.CurrentDate.AddDays(1);

            //var result = new GameDayResult
            //{
            //    NewDate = save.CurrentDate
            //};

            //// TODO: по-късно добавяме тук извиквания към:
            //// - мачове
            //// - тренировки
            //// - трансфери
            //// - финанси
            //// - съобщения и т.н.

            //// Пример: result.Events.Add("No matches today.");

            //await _context.SaveChangesAsync();
            //return result;
        }
    }
}
