using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Dashboards;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Specifications.Dashboards.GetDisbursementDetail;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FitBridge_Application.Features.Dashboards.GetDisbursementDetail
{
    internal class GetDisbursementDetailQueryHandler(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IUserUtil userUtil) : IRequestHandler<GetDisbursementDetailQuery, DashboardPagingResultDto<AvailableBalanceTransactionDto>>
    {
        public async Task<DashboardPagingResultDto<AvailableBalanceTransactionDto>> Handle(GetDisbursementDetailQuery request, CancellationToken cancellationToken)
        {
            var accountId = userUtil.GetAccountId(httpContextAccessor.HttpContext!)
                ?? throw new NotFoundException(nameof(ApplicationUser));

            var transactionSpec = new GetDisbursementDetailSpec(accountId, request.Params);
            var transactions = await unitOfWork.Repository<Transaction>()
                .GetAllWithSpecificationAsync(transactionSpec);

            var countSpec = new GetDisbursementDetailSpec(accountId, request.Params);
            var totalCount = await unitOfWork.Repository<Transaction>()
                .CountAsync(countSpec);

            var mappedTransactions = transactions.Select(transaction =>
            {
                return new AvailableBalanceTransactionDto
                {
                    OrderItemId = null,
                    CourseName = null,
                    TransactionId = transaction.Id,
                    TotalProfit = transaction.Amount,
                    TransactionType = transaction.TransactionType.ToString(),
                    ActualDistributionDate = null,
                    WithdrawDate = transaction.WithdrawalRequestId != null
                                   ? transaction.CreatedAt : null,
                    WithdrawalRequestId = transaction.WithdrawalRequestId,
                    Description = transaction.Description
                };
            }).ToList();

            var totalProfitSum = mappedTransactions.Sum(t => t.TotalProfit);

            return new DashboardPagingResultDto<AvailableBalanceTransactionDto>(totalCount, mappedTransactions, totalProfitSum);
        }
    }
}
