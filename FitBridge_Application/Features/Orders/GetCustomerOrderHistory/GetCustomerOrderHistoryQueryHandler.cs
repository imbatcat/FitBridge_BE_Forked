using AutoMapper;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Orders;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Specifications.Orders.GetCustomerOrderHistory;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FitBridge_Application.Features.Orders.GetCustomerOrderHistory
{
    public class GetCustomerOrderHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        IUserUtil userUtil,
        IHttpContextAccessor httpContextAccessor,
        IMapper _mapper)
        : IRequestHandler<GetCustomerOrderHistoryQuery, PagingResultDto<CustomerOrderHistoryDto>>
    {
        public async Task<PagingResultDto<CustomerOrderHistoryDto>> Handle(
            GetCustomerOrderHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var customerId = userUtil.GetAccountId(httpContextAccessor.HttpContext);
            if (customerId == null)
            {
                throw new NotFoundException(nameof(ApplicationUser));
            }

            var targetTransactionTypes = new[]
            {
                TransactionType.GymCourse,
                TransactionType.FreelancePTPackage,
                TransactionType.ExtendFreelancePTPackage,
                TransactionType.ExtendCourse,
                TransactionType.SubscriptionPlansOrder,
                TransactionType.RenewalSubscriptionPlansOrder
            };

            var spec = new GetCustomerOrderHistorySpec(request.Params, customerId.Value);
            
            var orders = await unitOfWork.Repository<Order>()
                .GetAllWithSpecificationAsync(spec);
            
            var totalCount = await unitOfWork.Repository<Order>().CountAsync(spec);

            var result = new List<CustomerOrderHistoryDto>();

            foreach (var order in orders)
            {
                var discountAmount = order.SubTotalPrice - order.TotalAmount;
                
                var orderDto = _mapper.Map<CustomerOrderHistoryDto>(order);
                orderDto.DiscountAmount = discountAmount;

                if (order.Coupon != null)
                {
                    orderDto.Coupon = _mapper.Map<CouponSummaryDto>(order.Coupon);
                }

                foreach (var orderItem in order.OrderItems)
                {
                    if (orderItem.FreelancePTPackageId != null || orderItem.GymCourseId != null)
                    {
                        var itemDto = _mapper.Map<OrderItemSummaryDto>(orderItem);

                        if (orderItem.FreelancePTPackageId != null && orderItem.FreelancePTPackage != null)
                        {
                            itemDto.ItemName = orderItem.FreelancePTPackage.Name;
                            itemDto.ImageUrl = orderItem.FreelancePTPackage.ImageUrl ?? string.Empty;
                        }
                        else if (orderItem.GymCourseId != null && orderItem.GymCourse != null)
                        {
                            itemDto.ItemName = orderItem.GymCourse.Name;
                            itemDto.ImageUrl = orderItem.GymCourse.ImageUrl ?? string.Empty;
                        }
                        else if (orderItem.UserSubscriptionId != null && orderItem.UserSubscription != null)
                        {
                            itemDto.ItemName = orderItem.UserSubscription.SubscriptionPlansInformation.PlanName;
                            itemDto.ImageUrl = orderItem.UserSubscription.SubscriptionPlansInformation.ImageUrl ?? string.Empty;
                        }

                        orderDto.Items.Add(itemDto);
                    }
                }

                var relevantTransactions = order.Transactions
                    .Where(t => targetTransactionTypes.Contains(t.TransactionType))
                    .ToList();

                foreach (var transaction in relevantTransactions)
                {
                    var transactionDto = _mapper.Map<CustomerTransactionDetailDto>(transaction);
                    orderDto.Transactions.Add(transactionDto);
                }

                orderDto.Transactions = orderDto.Transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();

                result.Add(orderDto);
            }

            return new PagingResultDto<CustomerOrderHistoryDto>(totalCount, result);
        }
    }
}

