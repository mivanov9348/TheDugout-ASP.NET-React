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
    string Sender,
    string SubjectTemplate,
    string BodyTemplate,
    string[] Placeholders
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

        public record EventTypeDto(
           string Code,
           string Name,
           int BaseSuccessRate
       );

        public record EventOutcomeDto(
            string Name,
            string EventTypeCode,
            bool ChangesPossession,
            int Weight,
            int RangeMin,
            int RangeMax
        );

        public record CommentaryTemplateDto(
            string EventTypeCode,
            string OutcomeName,
            string Template
        );

        public class EventAttributeWeightDto
        {
            public string EventTypeCode { get; set; } = string.Empty;
            public List<AttributeWeightDto> Attributes { get; set; } = new();
        }

        public class AttributeWeightDto
        {
            public string AttributeCode { get; set; } = string.Empty;
            public double Weight { get; set; }
        }

    }
}
