using System;
using FitBridge_Application.Dtos.SessionActivities;
using FitBridge_Application.Features.SessionActivities;
using FitBridge_Domain.Entities.Trainings;
using AutoMapper;

namespace FitBridge_Application.MappingProfiles;

public class SessionActivityMappingProfile : Profile
{
    public SessionActivityMappingProfile()
    {
        CreateMap<CreateSessionActivityCommand, SessionActivity>();
        CreateMap<SessionActivity, SessionActivityResponseDto>()
        .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
        .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.Name : null))
        .ForMember(dest => dest.VietnameseAssetName, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.VietNameseName : null))
        .ForMember(dest => dest.VietnameseAssetDescription, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.VietnameseDescription : null))
        .ForMember(dest => dest.AssetImage, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.MetadataImage : null));
    }
}
