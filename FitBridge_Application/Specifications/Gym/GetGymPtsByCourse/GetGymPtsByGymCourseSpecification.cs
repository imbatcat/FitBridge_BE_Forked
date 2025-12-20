using FitBridge_Application.Commons.Utils;
using FitBridge_Domain.Entities.Gyms;

namespace FitBridge_Application.Specifications.Gym.GetGymPtsByCourse
{
    public class GetGymPtsByGymCourseSpecification : BaseSpecification<GymCoursePT>
    {
        public GetGymPtsByGymCourseSpecification(
            Guid courseId,
            GetGymPtsByGymCourseParams parameters,
            bool includeUserDetails = true,
            bool includeUserGoalTraining = true) : base(x =>
            x.GymCourseId == courseId)
        {
            switch (StringCapitalizationConverter.ToUpperFirstChar(parameters.SortBy))
            {
                default:
                    if (parameters.SortOrder == "asc")
                        AddOrderBy(x => x.PT.FullName!);
                    else
                        AddOrderByDesc(x => x.PT.FullName!);
                    break;
            }
            AddInclude(x => x.PT);
            if (parameters.DoApplyPaging)
            {
                AddPaging((parameters.Page - 1) * parameters.Size, parameters.Size);
            }
            else
            {
                parameters.Size = -1;
                parameters.Page = -1;
            }

            if (includeUserDetails)
            {
                AddInclude(x => x.PT.UserDetail);
            }

            if (includeUserGoalTraining)
            {
                AddInclude(x => x.PT.GoalTrainings);
            }
        }
    }
}