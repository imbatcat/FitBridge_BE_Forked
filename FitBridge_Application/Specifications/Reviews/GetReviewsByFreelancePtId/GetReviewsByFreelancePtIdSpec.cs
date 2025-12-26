using FitBridge_Domain.Entities.MessageAndReview;

namespace FitBridge_Application.Specifications.Reviews.GetReviewsByFreelancePtId
{
    public class GetReviewsByFreelancePtIdSpec : BaseSpecification<Review>
    {
        public GetReviewsByFreelancePtIdSpec(Guid ptId) : base(x => x.FreelancePtId == ptId)
        {
        }
    }
}
