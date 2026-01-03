using FitBridge_Application.Services;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using Xunit;
using Moq;
using FitBridge_Application.Interfaces.Repositories;
using Quartz;
using FitBridge_Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FitBridge_UnitTest.Services
{
    public class TransactionServiceTests
    {
        [Theory]
        [InlineData(500000, 1, 0.15, 425000)] // Base case: no coupon
        [InlineData(500000, 2, 0.15, 850000)] // Multiple quantity
        [InlineData(500000, 1, 0.20, 400000)] // Higher commission
        public async Task CalculateMerchantProfit_WithoutCoupon_ShouldCalculateCorrectly(
            decimal price, int quantity, decimal commissionRate, decimal expectedProfit)
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<TransactionsService>>();
            var mockSchedulerFactory = new Mock<ISchedulerFactory>();
            var mockScheduleJobServices = new Mock<IScheduleJobServices>();
            var mockApplicationUserService = new Mock<IApplicationUserService>();
            
            // Create SystemConfigurationService with mock UnitOfWork
            var mockSystemConfigUnitOfWork = new Mock<IUnitOfWork>();
            var systemConfigurationService = new SystemConfigurationService(mockSystemConfigUnitOfWork.Object);

            var orderItem = new OrderItem
            {
                Price = price,
                Quantity = quantity,
                Order = new Order { CommissionRate = commissionRate }
            };

            var service = new TransactionsService(
                mockUnitOfWork.Object,
                mockLogger.Object,
                mockSchedulerFactory.Object,
                mockScheduleJobServices.Object,
                mockApplicationUserService.Object,
                systemConfigurationService
            );

            // Act
            var result = await service.CalculateMerchantProfit(orderItem, null);

            // Assert
            Assert.Equal(expectedProfit, result);
        }

        [Theory]
        [InlineData(500000, 10, 50000, CouponType.GymOwner, 0.15, 382500)]
        [InlineData(500000, 10, 50000, CouponType.System, 0.15, 425000)]
        public async Task CalculateMerchantProfit_WithCoupon_ShouldHandleTypeCorrectly(
            decimal price, double discountPercent, decimal maxDiscount,
            CouponType couponType, decimal commissionRate, decimal expectedProfit)
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<TransactionsService>>();
            var mockSchedulerFactory = new Mock<ISchedulerFactory>();
            var mockScheduleJobServices = new Mock<IScheduleJobServices>();
            var mockApplicationUserService = new Mock<IApplicationUserService>();
            var mockSystemConfigUnitOfWork = new Mock<IUnitOfWork>();
            var systemConfigurationService = new SystemConfigurationService(mockSystemConfigUnitOfWork.Object);

            var coupon = new Coupon
            {
                Type = couponType,
                DiscountPercent = discountPercent,
                MaxDiscount = maxDiscount
            };

            var orderItem = new OrderItem
            {
                Price = price,
                Quantity = 1,
                Order = new Order { CommissionRate = commissionRate, Coupon = coupon }
            };

            var service = new TransactionsService(
                mockUnitOfWork.Object,
                mockLogger.Object,
                mockSchedulerFactory.Object,
                mockScheduleJobServices.Object,
                mockApplicationUserService.Object,
                systemConfigurationService
            );

            // Act
            var result = await service.CalculateMerchantProfit(orderItem, coupon);

            // Assert
            Assert.Equal(expectedProfit, result);
        }
    }
}