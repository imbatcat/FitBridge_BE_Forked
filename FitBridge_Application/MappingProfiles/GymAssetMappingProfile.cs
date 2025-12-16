using AutoMapper;
using FitBridge_Application.Dtos.GymAssets;
using FitBridge_Domain.Entities.Gyms;

namespace FitBridge_Application.MappingProfiles;

public class GymAssetMappingProfile : Profile
{
    public GymAssetMappingProfile()
    {
        CreateMap<GymAsset, GymAssetResponseDto>()
            .ForMember(dest => dest.GymOwnerName, opt => opt.MapFrom(src => src.GymOwner.FullName))
            .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.AssetMetadata.Name))
            .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.AssetMetadata.AssetType))
            .ForMember(dest => dest.EquipmentCategory, opt => opt.MapFrom(src => src.AssetMetadata.EquipmentCategoryType))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.AssetMetadata.Description))
            .ForMember(dest => dest.TargetMuscularGroups, opt => opt.MapFrom(src => src.AssetMetadata.TargetMuscularGroups));
        CreateMap<AssetMetadata, AssetMetadataDto>();
        CreateProjection<AssetMetadata, AssetMetadataBrief>();
        CreateMap<AssetMetadata, SessionActivityAssetDto>()
        .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.VietNameseName, opt => opt.MapFrom(src => src.VietNameseName))
        .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
        .ForMember(dest => dest.VietnameseDescription, opt => opt.MapFrom(src => src.VietnameseDescription))
        .ForMember(dest => dest.TargetMuscularGroups, opt => opt.MapFrom(src => src.TargetMuscularGroups))
        .ForMember(dest => dest.MetadataImage, opt => opt.MapFrom(src => src.MetadataImage));
    }
}
