namespace TheDugout.Data.Seed
{
    public class SeedDtos
    {
        public record CountrySeedDto(string Code, string Name);

        public record LeagueTemplateSeedDto(
            string Code,
            string Name,
            string CountryCode,
            int Tier,
            int Teams,
            int RelegationSpots,
            int PromotionSpots
            );

        public record TeamTemplateSeedDto(
            string Name,
    string ShortName,
    string CompetitionCode
            );


    }
}
