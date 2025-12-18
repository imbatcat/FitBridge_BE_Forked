using AutoMapper;
using FitBridge_Application.Dtos.CustomerPurchaseds;
using FitBridge_Application.Dtos.FreelancePTPackages;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Specifications.CustomerPurchaseds.GetCustomerPurchasedForFreelancePt;
using FitBridge_Application.Specifications.OrderItems.GetOrderItemForFptDashboard;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FitBridge_Application.Features.CustomerPurchaseds.GetFreelancePtDashboard
{
    public class GetFreelancePtDashboardQueryHandler(
        IUnitOfWork unitOfWork,
        IUserUtil userUtil,
        IHttpContextAccessor httpContextAccessor,
        ITransactionService transactionService,
        ILogger<GetFreelancePtDashboardQueryHandler> _logger,
        IMapper mapper)
        : IRequestHandler<GetFreelancePtDashboardQuery, FreelancePtDashboardDto>
    {
        public async Task<FreelancePtDashboardDto> Handle(
            GetFreelancePtDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var ptId = userUtil.GetAccountId(httpContextAccessor.HttpContext);
            if (ptId == null)
            {
                throw new NotFoundException("PT ID not found");
            }
            string timeZoneId = Environment.OSVersion.Platform == PlatformID.Win32NT 
            ? "SE Asia Standard Time" 
            : "Asia/Ho_Chi_Minh";
    
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var utcNow = DateTime.UtcNow;
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localTimeZone);
            _logger.LogInformation("Local now: {LocalNow}", localNow);
            
            var currentDate = localNow;
            var currentMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = currentMonthStart.AddSeconds(-2);

            var activeCustomers = await unitOfWork.Repository<CustomerPurchased>().CountAsync(new GetCustomerPurchasedForFreelancePtSpec(ptId.Value, new GetCustomerPurchasedForFreelancePtParams { DoApplyPaging = false }));

            // Get all OrderItems for this PT's packages with necessary includes
            var allOrderItems = await unitOfWork.Repository<OrderItem>()
                .GetAllWithSpecificationAsync(new GetOrderItemForFptDashboardSpec(ptId.Value));
            // Calculate current month statistics
            var currentMonthStats = await CalculateMonthlyStatistics(
                allOrderItems.ToList(),
                currentMonthStart,
                currentDate,
                ptId.Value);

            // Calculate previous month statistics
            var previousMonthStats = await CalculateMonthlyStatistics(
                allOrderItems.ToList(),
                previousMonthStart,
                previousMonthEnd,
                ptId.Value);

            // Get most popular packages (all-time, top 3)
            var mostPopularPackages = await GetMostPopularPackages(allOrderItems.ToList(), ptId.Value);

            // Get package revenue breakdown (current month)
            var packageRevenueBreakdown = await GetPackageRevenueBreakdown(
                allOrderItems.Where(oi => oi.UpdatedAt >= currentMonthStart).ToList(),
                ptId.Value);

            return new FreelancePtDashboardDto
            {
                CurrentMonth = currentMonthStats,
                PreviousMonth = previousMonthStats,
                MostPopularPackages = mostPopularPackages,
                PackageRevenueBreakdown = packageRevenueBreakdown,
                CurrentActiveCustomers = activeCustomers,
            };
        }

        private async Task<MonthlyStatisticsDto> CalculateMonthlyStatistics(
            List<OrderItem> allOrderItems,
            DateTime periodStart,
            DateTime periodEnd,
            Guid ptId)
        {
            var periodOrderItems = allOrderItems
                .Where(oi => oi.UpdatedAt >= periodStart && oi.UpdatedAt <= periodEnd)
                .ToList();

            var numOfPurchases = periodOrderItems
                .Count(oi => oi.CustomerPurchased != null);
            // Calculate total revenue
            decimal totalRevenue = 0;
            decimal totalProfit = 0;

            foreach (var orderItem in periodOrderItems)
            {
                var itemRevenue = orderItem.Price * orderItem.Quantity;
                totalRevenue += itemRevenue;

                // Calculate profit using the TransactionService method
                var profit = await transactionService.CalculateMerchantProfit(orderItem, orderItem.Order?.Coupon);
                totalProfit += profit;
            }

            // Count new customers (customers who made their first purchase in this period)
            var totalCustomerIds = allOrderItems.GroupBy(oi => oi.CustomerPurchased.CustomerId).Select(group => new
            {
                CustomerId = group.Key,
                GroupCount = group.Count()
            }).ToList();
            var currentCustomerIds = periodOrderItems.Select(oi => oi.CustomerPurchased.CustomerId).Distinct().ToList();
            var newCustomers = totalCustomerIds.Where(x => currentCustomerIds.Contains(x.CustomerId) && x.GroupCount == 1).Count();
        

            return new MonthlyStatisticsDto
            {
                Year = periodStart.Year,
                Month = periodStart.Month,
                TotalPackagesSold = numOfPurchases,
                TotalRevenue = totalRevenue,
                TotalProfit = totalProfit,
                NewCustomers = newCustomers
            };
        }

        private async Task<List<PackageSalesStatDto>> GetMostPopularPackages(
            List<OrderItem> allOrderItems,
            Guid ptId)
        {
            // Group by package and calculate statistics
            var packageStats = allOrderItems
                .GroupBy(oi => oi.FreelancePTPackageId)
                .Select(g => new
                {
                    PackageId = g.Key.Value,
                    Package = g.First().FreelancePTPackage,
                    OrderItems = g.ToList()
                })
                .ToList();

            var result = new List<PackageSalesStatDto>();

            foreach (var packageGroup in packageStats)
            {
                var numOfPackagesSold = packageGroup.OrderItems
                    .Count(oi => oi.CustomerPurchased != null);

                decimal totalRevenue = 0;
                decimal totalProfit = 0;

                foreach (var orderItem in packageGroup.OrderItems)
                {
                    totalRevenue += orderItem.Price * orderItem.Quantity;
                    var profit = await transactionService.CalculateMerchantProfit(orderItem, orderItem.Order?.Coupon);
                    totalProfit += profit;
                }

                result.Add(new PackageSalesStatDto
                {
                    PackageId = packageGroup.PackageId,
                    PackageName = packageGroup.Package.Name,
                    PackageImageUrl = packageGroup.Package.ImageUrl,
                    PackagePrice = packageGroup.Package.Price,
                    TotalPackagesSold = numOfPackagesSold,
                    TotalRevenue = totalRevenue,
                    TotalProfit = totalProfit
                });
            }

            // Return top 3 by total sales
            return result.OrderByDescending(p => p.TotalPackagesSold).Take(3).ToList();
        }

        private async Task<List<PackageRevenueDto>> GetPackageRevenueBreakdown(
            List<OrderItem> periodOrderItems,
            Guid ptId)
        {
            if (!periodOrderItems.Any())
            {
                return new List<PackageRevenueDto>();
            }

            // Calculate total revenue for percentage calculation
            decimal totalPeriodRevenue = 0;
            var packageGroups = periodOrderItems
                .GroupBy(oi => oi.FreelancePTPackageId)
                .ToList();

            // First pass: calculate total revenue
            foreach (var group in packageGroups)
            {
                totalPeriodRevenue += group.Sum(oi => oi.Price * oi.Quantity);
            }

            var result = new List<PackageRevenueDto>();

            // Second pass: create breakdown with percentages
            foreach (var packageGroup in packageGroups)
            {
                var packageOrderItems = packageGroup.ToList();
                var package = packageOrderItems.First().FreelancePTPackage;

                decimal revenue = 0;
                decimal profit = 0;

                foreach (var orderItem in packageOrderItems)
                {
                    revenue += orderItem.Price * orderItem.Quantity;
                    var itemProfit = await transactionService.CalculateMerchantProfit(orderItem, orderItem.Order?.Coupon);
                    profit += itemProfit;
                }

                var revenuePercentage = totalPeriodRevenue > 0 
                    ? (double)(revenue / totalPeriodRevenue * 100) 
                    : 0;

                result.Add(new PackageRevenueDto
                {
                    PackageId = packageGroup.Key.Value,
                    PackageName = package.Name,
                    Revenue = revenue,
                    Profit = profit,
                    SalesCount = packageOrderItems.Count,
                    RevenuePercentage = Math.Round(revenuePercentage, 1)
                });
            }

            // Sort by revenue descending
            return result.OrderByDescending(p => p.Revenue).ToList();
        }
    }
}


