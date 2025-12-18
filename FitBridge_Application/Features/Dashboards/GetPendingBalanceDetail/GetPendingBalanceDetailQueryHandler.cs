using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Dashboards;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Specifications.Dashboards.GetOrderItemForPendingBalanceDetail;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FitBridge_Application.Features.Dashboards.GetPendingBalanceDetail
{
    internal class GetPendingBalanceDetailQueryHandler(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ITransactionService transactionService,
        IUserUtil userUtil) : IRequestHandler<GetPendingBalanceDetailQuery, DashboardPagingResultDto<PendingBalanceOrderItemDto>>
    {
        public async Task<DashboardPagingResultDto<PendingBalanceOrderItemDto>> Handle(GetPendingBalanceDetailQuery request, CancellationToken cancellationToken)
        {
            var accountId = userUtil.GetAccountId(httpContextAccessor.HttpContext!)
                ?? throw new NotFoundException(nameof(ApplicationUser));
            var accountRole = userUtil.GetUserRole(httpContextAccessor.HttpContext!)
                ?? throw new NotFoundException("User role");

            var orderItemSpec = new GetOrderItemForPendingBalanceDetailSpec(accountId, accountRole, request.Params);
            var orderItems = await unitOfWork.Repository<OrderItem>()
                .GetAllWithSpecificationAsync(orderItemSpec);

            var countSpec = new GetOrderItemForPendingBalanceDetailSpec(accountId, accountRole, request.Params);
            var totalCount = await unitOfWork.Repository<OrderItem>()
                .CountAsync(countSpec);
            var pendingDeductionTransactionList = new List<PendingBalanceOrderItemDto>();
            var tasks = orderItems.Select(async oi =>
            {
                var isGymOwner = accountRole == ProjectConstant.UserRoles.GymOwner;
                var profit = await transactionService.CalculateMerchantProfit(oi, oi.Order.Coupon);

                // Get the related transaction for this order item
                var relatedTransaction = oi.Order.Transactions
                    .FirstOrDefault(t => t.Status == TransactionStatus.Success && t.OrderId == oi.OrderId && t.TransactionType != TransactionType.PendingDeduction && t.TransactionType != TransactionType.DistributeProfit);

                TransactionDetailDto? transactionDetail = null;
                if (relatedTransaction != null)
                {
                    transactionDetail = new TransactionDetailDto
                    {
                        TransactionId = relatedTransaction.Id,
                        OrderCode = relatedTransaction.OrderCode,
                        TransactionDate = relatedTransaction.CreatedAt,
                        PaymentMethod = relatedTransaction.PaymentMethod.MethodType.ToString(),
                        Amount = relatedTransaction.Amount,
                        Description = relatedTransaction.Description
                    };
                }
                if (oi.Transactions.Any(t => t.TransactionType == TransactionType.PendingDeduction))
                {
                    pendingDeductionTransactionList.Add(new PendingBalanceOrderItemDto
                    {
                        OrderItemId = oi.Id,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        SubTotal = oi.Price * oi.Quantity,
                        TotalProfit = oi.Transactions.FirstOrDefault(t => t.TransactionType == TransactionType.PendingDeduction)?.Amount ?? 0,
                        CouponCode = oi.Order.Coupon?.CouponCode,
                        CouponDiscountPercent = oi.Order.Coupon?.DiscountPercent,
                        CouponId = oi.Order.CouponId,
                        CourseId = isGymOwner ? oi.GymCourseId!.Value : oi.FreelancePTPackageId!.Value,
                        CourseName = isGymOwner ? oi.GymCourse!.Name : oi.FreelancePTPackage!.Name,
                        CustomerId = oi.Order.AccountId,
                        CustomerFullName = oi.Order.Account.FullName,
                        PlannedDistributionDate = oi.ProfitDistributePlannedDate,
                        TransactionDetail = null,
                        TransactionType = TransactionType.PendingDeduction
                    });
                }
                return new PendingBalanceOrderItemDto
                {
                    OrderItemId = oi.Id,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    SubTotal = oi.Price * oi.Quantity,
                    TotalProfit = profit,
                    CouponCode = oi.Order.Coupon?.CouponCode,
                    CouponDiscountPercent = oi.Order.Coupon?.DiscountPercent,
                    CouponId = oi.Order.CouponId,
                    CourseId = isGymOwner ? oi.GymCourseId!.Value : oi.FreelancePTPackageId!.Value,
                    CourseName = isGymOwner ? oi.GymCourse!.Name : oi.FreelancePTPackage!.Name,
                    CustomerId = oi.Order.AccountId,
                    CustomerFullName = oi.Order.Account.FullName,
                    PlannedDistributionDate = oi.ProfitDistributePlannedDate,
                    TransactionDetail = transactionDetail,
                    TransactionType = relatedTransaction?.TransactionType
                };
            });

            var mappedOrderItems = await Task.WhenAll(tasks);
            var mappedOrderItemsList = mappedOrderItems.ToList();
            mappedOrderItemsList.AddRange(pendingDeductionTransactionList);
            var totalProfitSum = mappedOrderItemsList.Sum(oi => oi.TotalProfit);

            return new DashboardPagingResultDto<PendingBalanceOrderItemDto>(totalCount, mappedOrderItemsList, totalProfitSum);
        }
    }
}