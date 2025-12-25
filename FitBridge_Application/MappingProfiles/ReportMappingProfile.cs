using AutoMapper;
using FitBridge_Application.Dtos.Reports;
using FitBridge_Domain.Entities.Reports;
using Microsoft.Build.Tasks;

namespace FitBridge_Application.MappingProfiles
{
    public class ReportMappingProfile : Profile
    {
        public ReportMappingProfile()
        {
            CreateProjection<ReportCases, GetCustomerReportsResponseDto>()
           .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter.FullName))
              .ForMember(dest => dest.ReporterAvatarUrl, opt => opt.MapFrom(src => src.Reporter.AvatarUrl))
            .ForMember(dest => dest.ReportedUserName, opt => opt.MapFrom(src => src.ReportedUser != null ? src.ReportedUser.FullName : null))
            .ForMember(dest => dest.EvidenceImageUrls, opt => opt.MapFrom(src => src.ImageUrls))
            .ForMember(dest => dest.ReportedProduct, opt => opt.MapFrom(src =>
                src.OrderItem != null && src.OrderItem.ProductDetail != null && src.OrderItem.ProductDetail.Product != null && src.OrderItem.ProductDetail.Product.Name != null
                    ? src.OrderItem.ProductDetail.Product.Name
                    : (src.OrderItem != null && src.OrderItem.GymCourse != null
                        ? src.OrderItem.GymCourse.Name
                        : (src.OrderItem != null && src.OrderItem.FreelancePTPackage != null
                            ? src.OrderItem.FreelancePTPackage.Name
                            : null))))
            .ForMember(dest => dest.ResolvedEvidenceImageUrls, opt => opt.MapFrom(src => src.ResolvedEvidenceImageUrl))
            .ForMember(dest => dest.ReportedUserAvatarUrl, opt => opt.MapFrom(src => src.ReportedUser != null ? src.ReportedUser.AvatarUrl : null));

            CreateMap<ReportCases, GetCustomerReportsResponseDto>()
          .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter.FullName))
           .ForMember(dest => dest.ReporterAvatarUrl, opt => opt.MapFrom(src => src.Reporter.AvatarUrl))
           .ForMember(dest => dest.ReportedUserName, opt => opt.MapFrom(src => src.ReportedUser != null ? src.ReportedUser.FullName : null))
            .ForMember(dest => dest.EvidenceImageUrls, opt => opt.MapFrom(src => src.ImageUrls))
            .ForMember(dest => dest.ResolvedEvidenceImageUrls, opt => opt.MapFrom(src => src.ResolvedEvidenceImageUrl))
            .ForMember(dest => dest.ReportedProduct, opt => opt.MapFrom(src =>
                src.OrderItem != null && src.OrderItem.ProductDetail != null && src.OrderItem.ProductDetail.Product != null && src.OrderItem.ProductDetail.Product.Name != null
                    ? src.OrderItem.ProductDetail.Product.Name
                    : (src.OrderItem != null && src.OrderItem.GymCourse != null
                        ? src.OrderItem.GymCourse.Name
                        : (src.OrderItem != null && src.OrderItem.FreelancePTPackage != null
                            ? src.OrderItem.FreelancePTPackage.Name
                            : null))))
            .ForMember(dest => dest.ReportedUserAvatarUrl, opt => opt.MapFrom(src => src.ReportedUser != null ? src.ReportedUser.AvatarUrl : null));
        }
    }
}