namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Models.Staff;
    using static TheDugout.Data.Seed.SeedDtos;
    public static class SeedAgencies
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var agenciesPath = Path.Combine(seedDir, "agencies.json");
            var agencies = await SeedData.ReadJsonAsync<List<AgencyTemplateDto>>(agenciesPath);

            foreach (var agencyDto in agencies)
            {
                var existing = await db.AgencyTemplates
                    .FirstOrDefaultAsync(x => x.Name == agencyDto.Name);

                if (existing == null)
                {
                    var newAgency = new AgencyTemplate
                    {
                        Id = agencyDto.Id,
                        Name = agencyDto.Name,
                        RegionCode = agencyDto.RegionCode,
                        IsActive = agencyDto.IsActive
                    };

                    db.AgencyTemplates.Add(newAgency);
                }
                else
                {
                    existing.RegionCode = agencyDto.RegionCode;
                    existing.IsActive = agencyDto.IsActive;
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} agencies.", agencies.Count);
        }
    }
}
