using Common.Authority.Dto;
using Common.Authority.Entity;

namespace Common.Authority.Profile
{

    public class AuthorityAutoMapperProfile : AutoMapper.Profile
    {
        public AuthorityAutoMapperProfile()
        {
            CreateMap<Role, RoleDto>()
             .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
             .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name));

            CreateMap<AuthorityModule, AuthorityModuleDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name));

            CreateMap<RoleModule, AuthorityModuleDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.AuthorityModule.Name));
        }
    }
}