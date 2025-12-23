using System;

namespace FitBridge_Application.Specifications.Reviews.GetMerchantReview;

public class GetMerchantReviewParams : BaseParams
{
    public Guid? FreelancePtId { get; set; }
    public Guid? GymOwnerId { get; set; }
}
