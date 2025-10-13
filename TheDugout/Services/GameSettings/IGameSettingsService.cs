namespace TheDugout.Services.GameSettings
{
    public interface IGameSettingsService
    {
        Task<string?> GetValueAsync(string key);
        Task<int?> GetIntAsync(string key);
        Task<decimal?> GetDecimalAsync(string key);
        Task<bool> GetBoolAsync(string key);
        Task SetValueAsync(string key, string value); 
    }
}
