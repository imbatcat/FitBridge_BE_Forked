using System;

namespace FitBridge_Application.Dtos.Orders
{
    public class CouponSummaryDto
    {
        public Guid CouponId { get; set; }
        public required string CouponCode { get; set; }
        public double DiscountPercent { get; set; }
        public decimal MaxDiscount { get; set; }
    }
}

