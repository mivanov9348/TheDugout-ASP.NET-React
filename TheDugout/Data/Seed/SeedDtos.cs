using TheDugout.Models.Teams;

namespace TheDugout.Data.Seed
{
    public static class SeedDtos
    {
        public record CountryDto(
               string Code,
               string Name,
               string RegionCode 
           );

        public record LeagueTemplateDto(
            string Code,
            string Name,
            string CountryCode,
            int Tier,
            int Teams,
            int RelegationSpots,
            int PromotionSpots
        );

        public record CupTemplateDto(
    int Id,
    string Name,
    string CountryCode,
    bool IsActive,
    int? MinTeams,
    int? MaxTeams
);

        public record TeamTemplateDto(
            string Name,
            string ShortName,
            string? CompetitionCode,
            string? CountryCode
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

        public record AgencyTemplateDto(
    int Id,
    string Name,
    string Logo,
    string RegionCode,
    bool IsActive
);


    }
}
