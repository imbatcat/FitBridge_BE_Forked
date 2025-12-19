using FitBridge_Application.Commons.Utils;
using FitBridge_Application.Dtos.Orders;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using System;
using System.Linq;

namespace FitBridge_Application.Specifications.Orders.GetCustomerOrderHistory
{
    public class GetCustomerOrderHistorySpec : BaseSpecification<Order>
    {
        public GetCustomerOrderHistorySpec(
            GetCustomerOrderHistoryParams parameters,
            Guid customerId)
            : base(x =>
                x.IsEnabled
                && x.AccountId == customerId
                && x.Transactions.Any(t =>
                    t.TransactionType == TransactionType.GymCourse
                    || t.TransactionType == TransactionType.FreelancePTPackage
                    || t.TransactionType == TransactionType.ExtendFreelancePTPackage
                    || t.TransactionType == TransactionType.ExtendCourse)
                && (parameters.OrderId == null || x.Id == parameters.OrderId)
                && (parameters.OrderStatus == null || x.Status == parameters.OrderStatus.Value))
        {
            AddInclude(x => x.OrderItems);
            AddInclude("OrderItems.FreelancePTPackage");
            AddInclude("OrderItems.GymCourse");
            AddInclude(x => x.Transactions);
            AddInclude("Transactions.PaymentMethod");
            AddInclude(x => x.Coupon);

            switch (StringCapitalizationConverter.ToUpperFirstChar(parameters.SortBy))
            {
                case nameof(CustomerOrderHistoryDto.TotalAmount):
                    if (parameters.SortOrder == "asc")
                        AddOrderBy(x => x.TotalAmount);
                    else
                        AddOrderByDesc(x => x.TotalAmount);
                    break;

                case nameof(CustomerOrderHistoryDto.OrderStatus):
                    if (parameters.SortOrder == "asc")
                        AddOrderBy(x => x.Status);
                    else
                        AddOrderByDesc(x => x.Status);
                    break;

                case nameof(CustomerOrderHistoryDto.CreatedAt):
                    if (parameters.SortOrder == "asc")
                        AddOrderBy(x => x.CreatedAt);
                    else
                        AddOrderByDesc(x => x.CreatedAt);
                    break;

                default:
                    AddOrderByDesc(x => x.CreatedAt);
                    break;
            }

            if (parameters.DoApplyPaging)
            {
                AddPaging((parameters.Page - 1) * parameters.Size, parameters.Size);
            }
        }
    }
}

