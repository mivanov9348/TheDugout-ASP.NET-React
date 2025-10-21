namespace TheDugout.Services.GameSettings
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Common;
    using TheDugout.Services.GameSettings.Interfaces;

    public class GameSettingsService : IGameSettingsService
    {
        private readonly DugoutDbContext _context;
        private readonly Dictionary<string, string> _cache = new();

        public GameSettingsService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            // 1) Проверка в кеша
            if (_cache.TryGetValue(key, out var cachedValue))
                return cachedValue;

            // 2) Проверка в базата
            var setting = await _context.GameSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Key == key);

            if (setting == null)
                return null;

            // 3) Кеширане за следващи пъти
            _cache[key] = setting.Value;

            return setting.Value;
        }

        public async Task<int?> GetIntAsync(string key)
        {
            var val = await GetValueAsync(key);
            return int.TryParse(val, out var result) ? result : null;
        }

        public async Task<decimal?> GetDecimalAsync(string key)
        {
            var val = await GetValueAsync(key);
            return decimal.TryParse(val, out var result) ? result : null;
        }

        public async Task<bool> GetBoolAsync(string key)
        {
            var val = await GetValueAsync(key);
            return val != null && (val.Equals("true", StringComparison.OrdinalIgnoreCase) || val == "1");
        }

        public async Task SetValueAsync(string key, string value)
        {
            var setting = await _context.GameSettings.FirstOrDefaultAsync(x => x.Key == key);

            if (setting == null)
            {
                setting = new GameSetting { Key = key, Value = value };
                _context.GameSettings.Add(setting);
            }
            else
            {
                setting.Value = value;
                _context.GameSettings.Update(setting);
            }

            await _context.SaveChangesAsync();

            _cache[key] = value;
        }
    }
}
