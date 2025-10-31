namespace TheDugout.Services.Player.Interfaces
{
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Game;
    public interface IYouthPlayerService
    {
        Task GenerateYouthIntakeAsync(YouthAcademy academy, GameSave save);
    }
}
