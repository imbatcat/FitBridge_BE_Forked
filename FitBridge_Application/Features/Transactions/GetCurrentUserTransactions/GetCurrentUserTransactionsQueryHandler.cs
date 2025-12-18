using AutoMapper;
using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Transactions;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Specifications.Transactions.GetCurrentUserTransactions;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FitBridge_Application.Features.Transactions.GetCurrentUserTransactions
{
    internal class GetCurrentUserTransactionsQueryHandler(
        IUserUtil userUtil,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ITransactionService transactionService) : IRequestHandler<GetCurrentUserTransactionsQuery, PagingResultDto<GetTransactionsDto>>
    {
        public async Task<PagingResultDto<GetTransactionsDto>> Handle(GetCurrentUserTransactionsQuery request, CancellationToken cancellationToken)
        {
            var accountId = userUtil.GetAccountId(httpContextAccessor.HttpContext)
                    ?? throw new NotFoundException(nameof(ApplicationUser));

            var userRole = userUtil.GetUserRole(httpContextAccessor.HttpContext)
                ?? throw new NotFoundException("User role not found");

            var spec = new GetCurrentUserTransactionsSpec(request.Parameters, accountId, userRole);
            var transactions = await unitOfWork.Repository<Transaction>()
                .GetAllWithSpecificationAsync(spec);
            var totalCount = await unitOfWork.Repository<Transaction>()
                .CountAsync(spec);

            // Map using AutoMapper
            var transactionDtos = mapper.Map<List<GetTransactionsDto>>(transactions);
            
            // Populate role-specific fields for each transaction
            foreach (var dto in transactionDtos)
            {
                var transaction = transactions.FirstOrDefault(t => t.Id == dto.Id);
                if (transaction?.Order != null)
                {
                    OrderItem? orderItem = null;

                    if (userRole == ProjectConstant.UserRoles.GymOwner)
                    {
                        orderItem = transaction.Order.OrderItems.FirstOrDefault(x => x.GymCourse != null && x.GymCourse.GymOwnerId == accountId);
                    }
                    else if (userRole == ProjectConstant.UserRoles.FreelancePT)
                    {
                        orderItem = transaction.Order.OrderItems.FirstOrDefault(x => x.FreelancePTPackage != null && x.FreelancePTPackage.PtId == accountId);
                    }
                    else if (userRole == ProjectConstant.UserRoles.Customer || userRole == ProjectConstant.UserRoles.Admin)
                    {
                        orderItem = transaction.Order.OrderItems.FirstOrDefault();
                    }

                    if (orderItem != null)
                    {
                        // Set purchased item information
                        dto.PurchasedItemName = orderItem.GymCourse?.Name
                            ?? orderItem.FreelancePTPackage?.Name;

                        dto.PurchasedItemType = orderItem.GymCourse != null ? "Gym Course"
                            : orderItem.FreelancePTPackage != null ? "PT Package"
                            : orderItem.ProductDetail != null ? "Product"
                            : orderItem.SubscriptionPlansInformation != null ? "Subscription"
                            : "Other";

                        dto.CustomerPurchasedId = orderItem.CustomerPurchasedId;
                        dto.Quantity = orderItem.Quantity;

                        // Calculate profit amount
                        if (userRole == ProjectConstant.UserRoles.GymOwner || userRole == ProjectConstant.UserRoles.FreelancePT)
                        {
                            dto.ProfitAmount = await transactionService.CalculateMerchantProfit(
                                orderItem,
                                transaction.Order.Coupon);
                            dto.Amount = transaction.Order.Coupon != null ? transaction.Order.TotalAmount : orderItem.Price * orderItem.Quantity;
                        }
                    }
                    else
                    {
                        // For wallet/other transactions without order items
                        dto.PurchasedItemType = "Wallet/Other";
                    }
                }
                else
                {
                    // For transactions without orders (e.g., withdrawal, wallet top-up)
                    dto.PurchasedItemType = "Wallet/Other";
                }
            }

            return new PagingResultDto<GetTransactionsDto>(totalCount, transactionDtos);
        }
    }
}