using AutoMapper;
using FitBridge_Application.Dtos.Coupons;
using FitBridge_Application.Dtos.Orders;
using FitBridge_Domain.Entities.Orders;

namespace FitBridge_Application.MappingProfiles
{
    public class CouponMappingProfile : Profile
    {
        public CouponMappingProfile()
        {
            CreateMap<Coupon, CreateNewCouponDto>();
            CreateProjection<Coupon, GetCouponsDto>();
            CreateMap<Coupon, CouponSummaryDto>()
            .ForMember(dest => dest.CouponId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.CouponCode))
            .ForMember(dest => dest.DiscountPercent, opt => opt.MapFrom(src => src.DiscountPercent))
            .ForMember(dest => dest.MaxDiscount, opt => opt.MapFrom(src => src.MaxDiscount));
        }
    }
}