using MediatR;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Orders;
using FitBridge_Application.Specifications.Orders.GetCustomerOrderHistory;

namespace FitBridge_Application.Features.Orders.GetCustomerOrderHistory
{
    public class GetCustomerOrderHistoryQuery(GetCustomerOrderHistoryParams parameters) 
        : IRequest<PagingResultDto<CustomerOrderHistoryDto>>
    {
        public GetCustomerOrderHistoryParams Params { get; set; } = parameters;
    }
}

