using System;
using FitBridge_Domain.Entities.MessageAndReview;
using FitBridge_Application.Specifications;

namespace FitBridge_Application.Specifications.Reviews.GetMerchantReview;

public class GetMerchantReviewSpec : BaseSpecification<Review>
{
    public GetMerchantReviewSpec(GetMerchantReviewParams parameters) : base(x =>
    (parameters.FreelancePtId == null || x.FreelancePtId == parameters.FreelancePtId) &&
    (parameters.GymOwnerId == null || x.GymId == parameters.GymOwnerId))
    {
            AddInclude(x => x.User);
            AddInclude(x => x.Gym);
            AddInclude(x => x.FreelancePt);
        if (parameters.DoApplyPaging)
        {
            AddPaging(parameters.Size * (parameters.Page - 1), parameters.Size);
        }
        else
        {
            parameters.Size = -1;
            parameters.Page = -1;
        }
    }
}
