using FitBridge_Application.Dtos.Coupons;

namespace FitBridge_Application.Features.Reports.ConfirmReport
{
    public class ConfirmReportResponseDto
    {
        public decimal? CompletionPercentage { get; set; }

        public decimal OrderItemPrice { get; set; }

        public ApplyCouponDto? CouponDto { get; set; }

        public decimal RefundAmount { get; set; }
    }
}