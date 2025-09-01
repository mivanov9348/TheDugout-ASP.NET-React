namespace TheDugout.Services.Fixture
{
    public interface IFixturesService
    {
        Task GenerateFixturesAsync(int gameSaveId, int seasonId, DateTime startDate);

    }

}

