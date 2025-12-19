using System;
using System.Collections.Generic;

namespace FitBridge_Application.Dtos.Orders
{
    public class CustomerOrderHistoryDto
    {
        public Guid OrderId { get; set; }
        public required string OrderStatus { get; set; }
        public decimal SubTotalPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public CouponSummaryDto? Coupon { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemSummaryDto> Items { get; set; } = new();
        public List<CustomerTransactionDetailDto> Transactions { get; set; } = new();
    }
}

