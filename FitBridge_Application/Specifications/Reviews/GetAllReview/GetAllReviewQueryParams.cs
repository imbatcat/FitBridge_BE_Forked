using System;

namespace FitBridge_Application.Specifications.Reviews.GetAllReview;

public class GetAllReviewQueryParams : BaseParams
{
    public Guid? GymOwnerId { get; set; }
    public Guid? FreelancePtId { get; set; }
    public Guid? ProductId { get; set; }
}
