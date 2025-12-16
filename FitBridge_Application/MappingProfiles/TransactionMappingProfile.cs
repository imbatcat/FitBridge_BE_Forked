using AutoMapper;
using FitBridge_Application.Dtos.Transactions;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.MappingProfiles
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            CreateMap<Transaction, GetTransactionsDto>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.MethodType.ToString()))
                .ForMember(dest => dest.ProfitAmount, opt => opt.Ignore()) // Will be calculated manually in handler
                .ForMember(dest => dest.PurchasedItemName, opt => opt.Ignore()) // Will be set manually in handler based on user role
                .ForMember(dest => dest.PurchasedItemType, opt => opt.Ignore()) // Will be set manually in handler based on user role
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Order != null ? src.Order.Account.FullName : null))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Order != null ? (Guid?)src.Order.AccountId : null))
                .ForMember(dest => dest.CustomerPurchasedId, opt => opt.Ignore()) // Will be set manually in handler based on user role
                .ForMember(dest => dest.Quantity, opt => opt.Ignore()); // Will be set manually in handler based on user role
            CreateMap<Transaction, GetTransactionDetailDto>()
                .ForMember(x => x.PaymentMethod, opt => opt.MapFrom(y => y.PaymentMethod.MethodType));
            CreateMap<PaymentMethod, PaymentMethodDto>();
            CreateMap<WithdrawalRequest, WithdrawalRequestDto>();
            CreateMap<Transaction, GetAllTransactionAdminDto>()
                .ForMember(x => x.TransactionId, opt => opt.MapFrom(y => y.Id))
                .ForMember(x => x.PaymentMethod, opt => opt.MapFrom(y => y.PaymentMethod.MethodType))
                .ForMember(x => x.CustomerName, opt => opt.MapFrom(y => y.OrderId == null ? null : y.Order!.Account.FullName))
                .ForMember(x => x.CustomerAvatarUrl, opt => opt.MapFrom(y => y.OrderId == null ? null : y.Order!.Account.AvatarUrl))
                .ForMember(x => x.OrderId, opt => opt.MapFrom(y => y.OrderId))
                .ForMember(x => x.WalletId, opt => opt.MapFrom(y => y.WalletId))
                .ForMember(x => x.WithdrawalRequestId, opt => opt.MapFrom(y => y.WithdrawalRequestId));
        }
    }
}