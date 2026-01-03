using BenchmarkDotNet.Attributes;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Entities.Gyms;
using System;
using System.Collections.Generic;

namespace FitBridge_Benchmarks
{
    [MemoryDiagnoser]
    [MedianColumn]
    [MinColumn]
    [MaxColumn]
    public class DashboardQueryBenchmark
    {
        private List<OrderItem> _orderItems = null !;
        private List<Coupon> _coupons = null !;
        [Params(50, 100, 500)]
        public int OrderItemCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _orderItems = new List<OrderItem>();
            _coupons = new List<Coupon>();
            // Create sample coupons
            var systemCoupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Type = CouponType.System,
                DiscountPercent = 10,
                MaxDiscount = 50000
            };
            var merchantCoupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Type = CouponType.GymOwner,
                DiscountPercent = 15,
                MaxDiscount = 100000
            };
            _coupons.Add(systemCoupon);
            _coupons.Add(merchantCoupon);
            // Create sample order items
            for (int i = 0; i < OrderItemCount; i++)
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    CommissionRate = 0.15m,
                    Coupon = i % 3 == 0 ? _coupons[i % 2] : null
                };
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    Price = 500000 + (i * 1000),
                    Quantity = 1 + (i % 3),
                    Order = order,
                    FreelancePTPackageId = Guid.NewGuid(),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i % 30),
                    CustomerPurchased = new CustomerPurchased
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = Guid.NewGuid()
                    }
                };
                orderItem.FreelancePTPackage = new FreelancePTPackage
                {
                    Id = orderItem.FreelancePTPackageId.Value,
                    Name = $"Package {i % 10}",
                    ImageUrl = $"https://example.com/image{i % 10}.jpg",
                    Price = orderItem.Price
                };
                _orderItems.Add(orderItem);
            }
        }

        [Benchmark(Baseline = true)]
        public decimal CalculateProfitInLoop()
        {
            decimal totalProfit = 0;
            foreach (var orderItem in _orderItems)
            {
                var profit = CalculateMerchantProfit(orderItem, orderItem.Order?.Coupon);
                totalProfit += profit;
            }

            return totalProfit;
        }

        [Benchmark]
        public decimal CalculateProfitOptimized()
        {
            // Simulate optimized approach with batched calculation
            decimal totalProfit = 0;
            foreach (var orderItem in _orderItems)
            {
                var subTotalOrderItemPrice = orderItem.Price * orderItem.Quantity;
                var coupon = orderItem.Order?.Coupon;
                var commissionAmount = subTotalOrderItemPrice * orderItem.Order.CommissionRate;
                var merchantPtProfit = subTotalOrderItemPrice - commissionAmount;
                if (coupon != null && coupon.Type != CouponType.System)
                {
                    var discountAmount = subTotalOrderItemPrice * (decimal)(coupon.DiscountPercent / 100) > coupon.MaxDiscount ? coupon.MaxDiscount : subTotalOrderItemPrice * (decimal)(coupon.DiscountPercent / 100);
                    commissionAmount = (subTotalOrderItemPrice - discountAmount) * orderItem.Order.CommissionRate;
                    merchantPtProfit = subTotalOrderItemPrice - discountAmount - commissionAmount;
                }

                totalProfit += Math.Round(merchantPtProfit, 0, MidpointRounding.AwayFromZero);
            }

            return totalProfit;
        }

        private decimal CalculateMerchantProfit(OrderItem orderItem, Coupon? coupon)
        {
            var subTotalOrderItemPrice = orderItem.Price * orderItem.Quantity;
            var commissionAmount = subTotalOrderItemPrice * orderItem.Order.CommissionRate;
            var merchantPtProfit = Math.Round(subTotalOrderItemPrice - commissionAmount, 0, MidpointRounding.AwayFromZero);
            if (coupon != null)
            {
                if (coupon.Type != CouponType.System)
                {
                    var discountAmount = subTotalOrderItemPrice * (decimal)(coupon.DiscountPercent / 100) > coupon.MaxDiscount ? coupon.MaxDiscount : subTotalOrderItemPrice * (decimal)(coupon.DiscountPercent / 100);
                    commissionAmount = (subTotalOrderItemPrice - discountAmount) * orderItem.Order.CommissionRate;
                    merchantPtProfit = subTotalOrderItemPrice - discountAmount - commissionAmount;
                }
            }

            return Math.Round(merchantPtProfit, 0, MidpointRounding.AwayFromZero);
        }
    }
}