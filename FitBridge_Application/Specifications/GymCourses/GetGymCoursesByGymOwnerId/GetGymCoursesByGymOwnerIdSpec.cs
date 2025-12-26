using FitBridge_Domain.Entities.Gyms;

namespace FitBridge_Application.Specifications.GymCourses.GetGymCoursesByGymOwnerId
{
    public class GetGymCoursesByGymOwnerIdSpec : BaseSpecification<GymCourse>
    {
        public GetGymCoursesByGymOwnerIdSpec(Guid gymOwnerId) : base(x => x.GymOwnerId == gymOwnerId)
        {
            AddOrderBy(x => x.Price);
        }
    }
}
