using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Commons.Utils;
using FitBridge_Application.Dtos.Transactions;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Specifications.Transactions.GetCurrentUserTransactions
{
    public class GetCurrentUserTransactionsSpec : BaseSpecification<Transaction>
    {
        public GetCurrentUserTransactionsSpec(
            GetCurrentUserTransactionsParam parameters,
            Guid userId,
            string userRole,
            bool includeOrder = false) : base(x =>
            x.IsEnabled
            && x.TransactionType != TransactionType.PendingDeduction && x.TransactionType != TransactionType.DistributeProfit
            && (userRole == ProjectConstant.UserRoles.Admin
                || (userRole == ProjectConstant.UserRoles.GymOwner
                    && x.Order != null && x.Order.OrderItems.Any(oi => oi.GymCourse != null && oi.GymCourse.GymOwnerId == userId))
                || (userRole == ProjectConstant.UserRoles.FreelancePT
                    && x.Order != null && x.Order.OrderItems.Any(oi => oi.FreelancePTPackage != null && oi.FreelancePTPackage.PtId == userId))
                || (userRole == ProjectConstant.UserRoles.Customer
                    && x.Order != null && x.Order.AccountId == userId)))
        {
            switch (StringCapitalizationConverter.ToUpperFirstChar(parameters.SortBy))
            {
                case nameof(GetTransactionsDto.Amount):
                    if (parameters.SortOrder == "asc")
                        AddOrderBy(x => x.Amount);
                    else
                        AddOrderByDesc(x => x.Amount);
                    break;

                case nameof(GetTransactionsDto.CreatedAt):
                    if (parameters.SortOrder == "asc")
                        AddOrderBy(x => x.CreatedAt);
                    else
                        AddOrderByDesc(x => x.CreatedAt);
                    break;

                case nameof(GetTransactionsDto.Status):
                    if (parameters.SortOrder == "asc")
                        AddOrderBy(x => x.Status);
                    else
                        AddOrderByDesc(x => x.Status);
                    break;

                default:
                    AddOrderBy(x => x.CreatedAt);
                    break;
            }

            if (parameters.DoApplyPaging)
            {
                AddPaging(parameters.Size * (parameters.Page - 1), parameters.Size);
            }

            // Add necessary includes for transaction details
            AddInclude(x => x.PaymentMethod);
            AddInclude(x => x.OrderItem!);
            AddInclude("Order.OrderItems");
            AddInclude("Order.Account");
            AddInclude("Order.Coupon");
            AddInclude("Order.OrderItems.GymCourse");
            AddInclude("Order.OrderItems.FreelancePTPackage");
            AddInclude("Order.OrderItems.ProductDetail.Product");
            AddInclude("Order.OrderItems.SubscriptionPlansInformation");
            AddInclude("Order.OrderItems.CustomerPurchased");
            
            if (includeOrder)
            {
                AddInclude(x => x.Order!);
            }
        }
    }
}