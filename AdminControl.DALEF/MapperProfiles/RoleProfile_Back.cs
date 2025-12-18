using AutoMapper;
using AdminControl.DALEF.Models;
using AdminControl.DTO;

namespace AdminControl.DALEF.MapperProfiles
{
    public class RoleProfile_Back : Profile
    {
        public RoleProfile_Back()
        {
            // DTO -> Entity
            CreateMap<RoleCreateDto, Role>()
                .ForMember(dest => dest.RoleID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<RoleUpdateDto, Role>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // Entity -> DTO
            CreateMap<Role, RoleDto>();
        }
    }
}
