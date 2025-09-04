using AutoMapper;
using TheDugout.DTOs.Player;
using TheDugout.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Player -> PlayerDto
        CreateMap<Player, PlayerDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Position,
                opt => opt.MapFrom(src => src.Position.Name))
            .ForMember(dest => dest.Country,
                opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : ""));

        // PlayerAttribute -> PlayerAttributeDto
        CreateMap<PlayerAttribute, PlayerAttributeDto>()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Attribute.Name));

        // PlayerSeasonStats -> PlayerSeasonStatsDto
        CreateMap<PlayerSeasonStats, PlayerSeasonStatsDto>();


    }
}
