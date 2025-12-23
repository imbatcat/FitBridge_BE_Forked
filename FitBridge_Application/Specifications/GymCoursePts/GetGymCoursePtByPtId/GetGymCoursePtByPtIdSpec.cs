using System;
using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Gyms;

namespace FitBridge_Application.Specifications.GymCoursePts.GetGymCoursePtByPtId;

public class GetGymCoursePtByPtIdSpec : BaseSpecification<GymCoursePT>
{
    public GetGymCoursePtByPtIdSpec(Guid ptId) : base(x => x.PTId == ptId)
    {
    }
}
