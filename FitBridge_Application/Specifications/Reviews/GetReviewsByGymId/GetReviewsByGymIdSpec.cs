using FitBridge_Domain.Entities.MessageAndReview;

namespace FitBridge_Application.Specifications.Reviews.GetReviewsByGymId
{
    public class GetReviewsByGymIdSpec : BaseSpecification<Review>
    {
        public GetReviewsByGymIdSpec(Guid gymId) : base(x => x.GymId == gymId)
        {
        }
    }
}
