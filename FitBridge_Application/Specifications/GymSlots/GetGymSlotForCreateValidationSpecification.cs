using System;
using System.Security.Cryptography.X509Certificates;
using FitBridge_Application.Dtos.GymSlots;
using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Gyms;

namespace FitBridge_Application.Specifications.GymSlots;

public class GetGymSlotForCreateValidationSpecification : BaseSpecification<GymSlot>
{
    public GetGymSlotForCreateValidationSpecification(Guid gymOwnerId, string name, TimeOnly startTime, TimeOnly endTime)
        : base(x =>
            x.IsEnabled
            && x.GymOwnerId == gymOwnerId
            && (x.Name == name
                || (x.StartTime < endTime && x.EndTime > startTime)))
    {
    }
}
