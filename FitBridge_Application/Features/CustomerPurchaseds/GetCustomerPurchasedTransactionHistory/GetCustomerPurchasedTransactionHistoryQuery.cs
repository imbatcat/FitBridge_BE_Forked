using FitBridge_Application.Dtos.CustomerPurchaseds;
using MediatR;

namespace FitBridge_Application.Features.CustomerPurchaseds.GetCustomerPurchasedTransactionHistory;

public class GetCustomerPurchasedTransactionHistoryQuery : IRequest<CustomerPurchasedTransactionHistoryDto>
{
    public Guid CustomerPurchasedId { get; set; }
}

