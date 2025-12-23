using FitBridge_Domain.Entities.Orders;

namespace FitBridge_Application.Specifications.Orders.GetOrderItemById
{
    public class GetOrderItemWithCourseCompletionSpec : BaseSpecification<OrderItem>
    {
        public GetOrderItemWithCourseCompletionSpec(Guid orderItemId) : base(x => x.Id == orderItemId)
        {
            AddInclude(x => x.GymCourse);
            AddInclude(x => x.FreelancePTPackage);
            AddInclude(x => x.CustomerPurchased);
            AddInclude("CustomerPurchased.Bookings");
        }
    }
}
