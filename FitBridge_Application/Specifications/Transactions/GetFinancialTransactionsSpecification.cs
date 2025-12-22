using System;
using System.Linq.Expressions;
using FitBridge_Application.Features.Dashboards.GetDashboardFinancialStats;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Specifications.Transactions;

public class GetFinancialTransactionsSpecification : BaseSpecification<Transaction>
{
    public GetFinancialTransactionsSpecification(GetDashboardFinancialStatsParams parameters) 
        : base(x => (parameters.FromDate == null || x.CreatedAt >= parameters.FromDate.Value)
        && (parameters.ToDate == null || x.CreatedAt <= parameters.ToDate.Value)
        && x.IsEnabled
        && x.Status == TransactionStatus.Success
        && x.TransactionType != TransactionType.PendingDeduction
        && x.TransactionType != TransactionType.DistributeProfit
        && x.TransactionType != TransactionType.Withdraw
        && x.TransactionType != TransactionType.AssignPt
        && x.TransactionType != TransactionType.Disbursement
        && x.Order != null
        )
    {
        AddInclude(t => t.Order);
        AddInclude(t => t.PaymentMethod);
        AddInclude("Order.OrderItems");
        AddInclude("Order.OrderItems.ProductDetail");
        AddInclude("Order.OrderItems.ProductDetail.Product");
        AddInclude("Order.OrderItems.GymCourse");
        AddInclude("Order.OrderItems.FreelancePTPackage");
        AddInclude("Order.OrderItems.UserSubscription");
        AddInclude("Order.Account");
        AddInclude("Order.Coupon");
        
        AddOrderByDesc(t => t.CreatedAt);
    }
}

