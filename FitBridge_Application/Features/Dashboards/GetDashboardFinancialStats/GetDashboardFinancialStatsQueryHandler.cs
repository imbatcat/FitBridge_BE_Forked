using System;
using System.Linq;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Dashboard;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Specifications.Transactions;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitBridge_Application.Features.Dashboards.GetDashboardFinancialStats;

public class GetDashboardFinancialStatsQueryHandler(IUnitOfWork _unitOfWork) 
    : IRequestHandler<GetDashboardFinancialStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(GetDashboardFinancialStatsQuery request, CancellationToken cancellationToken)
    {
        var parameters = request.Params;
        
        // Get base transactions with filters
        var specification = new GetFinancialTransactionsSpecification(parameters);
        var transactions = await _unitOfWork.Repository<Transaction>().GetAllWithSpecificationAsync(specification);
        
        var validTransactions = transactions.ToList();

        // Calculate metrics by category
        var productMetrics = CalculateProductMetrics(validTransactions);
        var gymMetrics = CalculateGymMetrics(validTransactions);
        var freelanceMetrics = CalculateFreelanceMetrics(validTransactions);
        var subscriptionMetrics = CalculateSubscriptionMetrics(validTransactions);

        // Build summary cards
        var summaryCards = new SummaryCardsDto
        {
            TotalRevenue = productMetrics.Revenue + gymMetrics.Revenue + freelanceMetrics.Revenue + subscriptionMetrics.Revenue,
            TotalProfit = productMetrics.Profit + gymMetrics.Profit + freelanceMetrics.Profit + subscriptionMetrics.Profit,
            TotalProductProfit = productMetrics.Profit,
            TotalGymCourseProfit = gymMetrics.Profit,
            TotalFreelanceProfit = freelanceMetrics.Profit,
            TotalSubscriptionProfit = subscriptionMetrics.Profit
        };


        // Build profit distribution
        var totalProfit = summaryCards.TotalProfit;

        var productPercentage = totalProfit > 0 ? Math.Round((double)(productMetrics.Profit / totalProfit * 100), 0, MidpointRounding.AwayFromZero) : 0;
        var gymPercentage = totalProfit > 0 ? Math.Round((double)(gymMetrics.Profit / totalProfit * 100), 0, MidpointRounding.AwayFromZero) : 0;
        var freelancePercentage = totalProfit > 0 ? Math.Round((double)(freelanceMetrics.Profit / totalProfit * 100), 0, MidpointRounding.AwayFromZero) : 0;
        var subscriptionPercentage = totalProfit > 0 ? Math.Round((double)(subscriptionMetrics.Profit / totalProfit * 100), 0, MidpointRounding.AwayFromZero) : 0;

        var profitDistribution = new List<ProfitDistributionDto>
        {
            new() { Label = "Product", Value = productMetrics.Profit, Percentage = productPercentage },
            new() { Label = "Gym Course", Value = gymMetrics.Profit, Percentage = gymPercentage },
            new() { Label = "Freelance PT", Value = freelanceMetrics.Profit, Percentage = freelancePercentage },
            new() { Label = "Subscription", Value = subscriptionMetrics.Profit, Percentage = subscriptionPercentage }
        };

        // Build over time chart
        // var chartPoints = BuildOverTimeChart(validTransactions);

        // Build recent transactions table (paginated)
        var recentTransactionsQuery = validTransactions
            .OrderByDescending(t => t.CreatedAt)
            .AsQueryable();

        var totalCount = recentTransactionsQuery.Count();
        var paginatedTransactions = recentTransactionsQuery;
        if(parameters.DoApplyPaging) {
            paginatedTransactions = paginatedTransactions
                .Skip((parameters.Page - 1) * parameters.Size)
                .Take(parameters.Size);
        }

        var recentTransactionDtos = paginatedTransactions.Select(MapToRecentTransactionDto).ToList();

        return new DashboardStatsDto
        {
            SummaryCards = summaryCards,
            ProfitDistributionChart = profitDistribution,
            // RevenueProfitOverTimeChart = chartPoints,
            RecentTransactionsTable = new PagingResultDto<RecentTransactionDto>(totalCount, recentTransactionDtos)
        };
    }

    private CategoryMetrics CalculateProductMetrics(List<Transaction> transactions)
    {
        var productTransactions = transactions
            .Where(t => t.TransactionType == TransactionType.ProductOrder)
            .ToList();

        decimal revenue = 0;
        decimal profit = 0;

        foreach (var transaction in productTransactions)
        {
            var order = transaction.Order;
            if (order == null) continue;

            // Calculate revenue
            var orderRevenue = order.TotalAmount;
            var refundedAmount = order.OrderItems
                .Where(oi => oi.IsRefunded && oi.ProductDetailId != null)
                .Sum(oi => oi.Price * oi.Quantity);
            revenue += orderRevenue - refundedAmount;

            // Calculate profit
            var validProductItems = order.OrderItems
                .Where(oi => oi.ProductDetailId != null && !oi.IsRefunded)
                .ToList();

            var profitRefundedAmount = order.OrderItems.Where(oi => oi.IsRefunded && oi.ProductDetailId != null).Sum(oi => (oi.Price - (oi.OriginalProductPrice ?? 0)) * oi.Quantity);
            profit += (transaction.ProfitAmount ?? 0) - profitRefundedAmount;
        

            // Subtract shipping loss for cancelled COD orders
            if (order.Status == OrderStatus.Cancelled
                && transaction.PaymentMethod?.MethodType == MethodType.COD
                && order.ShippingFeeActualCost.HasValue)
            {
                profit -= order.ShippingFeeActualCost.Value;
            }
        }

        revenue = Math.Round(revenue, 0, MidpointRounding.AwayFromZero);
        profit = Math.Round(profit, 0, MidpointRounding.AwayFromZero);
        return new CategoryMetrics { Revenue = revenue, Profit = profit };
    }

    private CategoryMetrics CalculateGymMetrics(List<Transaction> transactions)
    {
        var gymTransactionTypes = new[] 
        { 
            TransactionType.GymCourse, 
            TransactionType.ExtendCourse 
        };

        var gymTransactions = transactions
            .Where(t => gymTransactionTypes.Contains(t.TransactionType))
            .ToList();

        decimal revenue = 0;
        decimal profit = 0;

        foreach (var transaction in gymTransactions)
        {
            var order = transaction.Order;
            if (order == null) continue;

            // Revenue calculation
            var orderRevenue = order.TotalAmount;
            var refundedAmount = order.OrderItems
                .Where(oi => oi.IsRefunded && oi.GymCourseId != null)
                .Sum(oi => oi.Price * oi.Quantity);
            revenue += orderRevenue - refundedAmount;

            // Profit = System commission (ProfitAmount)
            profit += transaction.ProfitAmount ?? 0;

            // Subtract commission lost from refunded items
            var refundedGymItems = order.OrderItems
                .Where(oi => oi.IsRefunded && oi.GymCourseId != null)
                .ToList();

            foreach (var refundedItem in refundedGymItems)
            {
                var commissionLost = CalculateCommission(refundedItem, order);
                profit -= commissionLost;
            }
        }

        revenue = Math.Round(revenue, 0, MidpointRounding.AwayFromZero);
        profit = Math.Round(profit, 0, MidpointRounding.AwayFromZero);
        return new CategoryMetrics { Revenue = revenue, Profit = profit };
    }

    private CategoryMetrics CalculateFreelanceMetrics(List<Transaction> transactions)
    {
        var freelanceTransactionTypes = new[] 
        { 
            TransactionType.FreelancePTPackage, 
            TransactionType.ExtendFreelancePTPackage 
        };

        var freelanceTransactions = transactions
            .Where(t => freelanceTransactionTypes.Contains(t.TransactionType))
            .ToList();

        decimal revenue = 0;
        decimal profit = 0;

        foreach (var transaction in freelanceTransactions)
        {
            var order = transaction.Order;
            if (order == null) continue;

            // Revenue calculation
            var orderRevenue = order.TotalAmount;
            var refundedAmount = order.OrderItems
                .Where(oi => oi.IsRefunded && oi.FreelancePTPackageId != null)
                .Sum(oi => oi.Price * oi.Quantity);
            revenue += (orderRevenue - refundedAmount);

            // Profit = System commission (ProfitAmount)
            profit += transaction.ProfitAmount ?? 0;

            // Subtract commission lost from refunded items
            var refundedPTItems = order.OrderItems
                .Where(oi => oi.IsRefunded && oi.FreelancePTPackageId != null)
                .ToList();

            foreach (var refundedItem in refundedPTItems)
            {
                var commissionLost = CalculateCommission(refundedItem, order);
                profit -= commissionLost;
            }
        }

        revenue = Math.Round(revenue, 0, MidpointRounding.AwayFromZero);
        profit = Math.Round(profit, 0, MidpointRounding.AwayFromZero);
        return new CategoryMetrics { Revenue = revenue, Profit = profit };
    }

    private CategoryMetrics CalculateSubscriptionMetrics(List<Transaction> transactions)
    {
        var subscriptionTransactionTypes = new[] 
        { 
            TransactionType.SubscriptionPlansOrder, 
            TransactionType.PurchasePremiumService,
            TransactionType.RenewalSubscriptionPlansOrder
        };

        var subscriptionTransactions = transactions
            .Where(t => subscriptionTransactionTypes.Contains(t.TransactionType))
            .ToList();

        var revenue = subscriptionTransactions
            .Where(t => t.Order != null)
            .Sum(t => t.Order!.TotalAmount);

        var profit = subscriptionTransactions
            .Sum(t => t.ProfitAmount ?? 0);
        revenue = Math.Round(revenue, 1, MidpointRounding.AwayFromZero);
        profit = Math.Round(profit, 1, MidpointRounding.AwayFromZero);
        return new CategoryMetrics { Revenue = revenue, Profit = profit };
    }

    private decimal CalculateCommission(OrderItem orderItem, Order order)
    {
        var subTotalPrice = orderItem.Price * orderItem.Quantity;
        var commissionAmount = subTotalPrice * order.CommissionRate;

        if (order.Coupon != null && order.Coupon.Type != CouponType.System)
        {
            var discountPercent = (decimal)(order.Coupon.DiscountPercent / 100);
            var discountAmount = subTotalPrice * discountPercent;
            if (discountAmount > order.Coupon.MaxDiscount)
            {
                discountAmount = order.Coupon.MaxDiscount;
            }
            commissionAmount = (subTotalPrice - discountAmount) * order.CommissionRate;
        }
        if (order.Coupon != null && order.Coupon.Type == CouponType.System)
        {
            var discountAmount = subTotalPrice * (decimal)(order.Coupon.DiscountPercent / 100) > order.Coupon.MaxDiscount ? order.Coupon.MaxDiscount : subTotalPrice * (decimal)(order.Coupon.DiscountPercent / 100);
            commissionAmount = commissionAmount - discountAmount;
        }

        return Math.Round(commissionAmount, 0, MidpointRounding.AwayFromZero);
    }

    // private List<ChartPointDto> BuildOverTimeChart(List<Transaction> transactions)
    // {
    //     var groupedByDate = transactions
    //         .GroupBy(t => t.CreatedAt.Date)
    //         .OrderBy(g => g.Key)
    //         .ToList();

    //     var chartPoints = new List<ChartPointDto>();

    //     foreach (var group in groupedByDate)
    //     {
    //         var dayTransactions = group.ToList();
            
    //         var productMetrics = CalculateProductMetrics(dayTransactions);
    //         var gymMetrics = CalculateGymMetrics(dayTransactions);
    //         var freelanceMetrics = CalculateFreelanceMetrics(dayTransactions);
    //         var subscriptionMetrics = CalculateSubscriptionMetrics(dayTransactions);

    //         chartPoints.Add(new ChartPointDto
    //         {
    //             Date = group.Key,
    //             ProductRevenue = productMetrics.Revenue,
    //             ProductProfit = productMetrics.Profit,
    //             GymRevenue = gymMetrics.Revenue,
    //             GymProfit = gymMetrics.Profit,
    //             FreelanceRevenue = freelanceMetrics.Revenue,
    //             FreelanceProfit = freelanceMetrics.Profit,
    //             SubRevenue = subscriptionMetrics.Revenue,
    //             SubProfit = subscriptionMetrics.Profit
    //         });
    //     }

    //     return chartPoints;
    // }

    private RecentTransactionDto MapToRecentTransactionDto(Transaction transaction)
    {
        var order = transaction.Order;

        return new RecentTransactionDto
        {
            TransactionId = transaction.Id,
            OrderCode = transaction.OrderCode,
            TotalAmount = order.TotalAmount,
            ProfitAmount = transaction.ProfitAmount,
            Status = transaction.Status.ToString(),
            CreatedAt = transaction.CreatedAt,
            OrderTotalAmount = order?.TotalAmount ?? 0,
            CustomerFullName = order?.Account?.FullName ?? "N/A",
            CustomerContact = order?.Account?.Email ?? order?.Account?.PhoneNumber ?? "N/A",
            TransactionType = transaction.TransactionType,
            OrderId = order.Id
        };
    }
}

