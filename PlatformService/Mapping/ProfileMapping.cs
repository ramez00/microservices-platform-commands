using System.Runtime.CompilerServices;
using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Grpc;

namespace PlatformService.Mapping
{
    public class ProfileMapping : Profile
    {
        public ProfileMapping()
        {
            // Mapping configurations
            // source -> target
            CreateMap<Platform, PlatformDto>().ReverseMap();
            CreateMap<Platform, PlatformPublishedDto>();
            CreateMap<Platform,GrpcPlatformModel>()
                .ForMember(dest => dest.PlatformId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher));
        }
    }
}