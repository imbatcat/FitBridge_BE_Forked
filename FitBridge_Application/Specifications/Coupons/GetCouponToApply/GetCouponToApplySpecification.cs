using FitBridge_Domain.Entities.Orders;

namespace FitBridge_Application.Specifications.Coupons.GetCouponToApply
{
    public class GetCouponToApplySpecification : BaseSpecification<Coupon>
    {
        public GetCouponToApplySpecification(string couponCode,
            bool isIncludeCreator = false) : base(x => x.IsActive
                                                       && x.IsEnabled
                                                       && x.CouponCode.ToLower().Equals(couponCode.ToLower()))
        {
            if (isIncludeCreator)
            {
                AddInclude(x => x.Creator);
            }
        }
    }
}