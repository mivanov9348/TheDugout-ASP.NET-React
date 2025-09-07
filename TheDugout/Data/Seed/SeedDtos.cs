using TheDugout.Models;

namespace TheDugout.Data.Seed
{
    public static class SeedDtos
    {
        public record CountryDto(string Code, string Name);

        public record LeagueTemplateDto(
            string Code,
            string Name,
            string CountryCode,
            int Tier,
            int Teams,
            int RelegationSpots,
            int PromotionSpots
        );

        public record TeamTemplateDto(
            string Name,
            string ShortName,
            string CompetitionCode
        );

        public record PositionWeightDto(
            string PositionCode,
            string AttributeCode,
            double Weight
        );

        public record MessageTemplateDto(
    string Category,
    string SubjectTemplate,
    string BodyTemplate,
    string[] Placeholders,
    int Weight,
    bool IsActive,
    string Language
);

        public record TacticDto(
           int Id,
           string Name,
           int Defenders,
           int Midfielders,
           int Forwards,
           List<TeamTactic> TeamTactics
       );

    }
}
