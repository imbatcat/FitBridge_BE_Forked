using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Dashboards;
using FitBridge_Application.Specifications.Dashboards.GetDisbursementDetail;
using MediatR;

namespace FitBridge_Application.Features.Dashboards.GetDisbursementDetail
{
    public class GetDisbursementDetailQuery(GetDisbursementDetailParams parameters) : IRequest<DashboardPagingResultDto<AvailableBalanceTransactionDto>>
    {
        public GetDisbursementDetailParams Params { get; set; } = parameters;
    }
}
