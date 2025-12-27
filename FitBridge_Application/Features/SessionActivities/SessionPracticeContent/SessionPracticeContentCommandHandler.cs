using System;
using FitBridge_Application.Dtos.SessionActivities;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Specifications.SessionActivities.GetSessionActivitiesByBookingId;
using FitBridge_Domain.Entities.Trainings;
using FitBridge_Domain.Enums.Gyms;
using MediatR;

namespace FitBridge_Application.Features.SessionActivities.SessionPracticeContent;

public class SessionPracticeContentCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<SessionPracticeContentCommand, SessionPracticeContentDto>
{
    public async Task<SessionPracticeContentDto> Handle(SessionPracticeContentCommand request, CancellationToken cancellationToken)
    {
        var sessionActivities = await _unitOfWork.Repository<SessionActivity>().GetAllWithSpecificationAsync(new GetSessionActivitiesByBookingIdSpecification(request.BookingId), true);
        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(request.BookingId, includes: new List<string> { "Customer" });
        if (sessionActivities.Count == 0)
        {
            return new SessionPracticeContentDto { SessionActivities = new List<SessionActivityListDto>(), BookingName = "" };
        }
        var sessionActivitiesDtos = new List<SessionActivityListDto>();
        foreach (var sessionActivity in sessionActivities)
        {
            var sessionActivityDto = new SessionActivityListDto
            {
                Id = sessionActivity.Id,
                ActivityName = sessionActivity.ActivityName,
                ActivityType = sessionActivity.ActivityType,
                MuscleGroup = sessionActivity.MuscleGroup,
                NumOfReps = sessionActivity.ActivitySets.Sum(x => x.NumOfReps ?? 0),
                WeightLifted = sessionActivity.ActivitySets.Sum(x => x.WeightLifted * x.NumOfReps ?? 0),
                TotalSets = sessionActivity.ActivitySets.Count,
                ActivitySetType = sessionActivity.ActivitySetType,
                TotalPlannedNumOfReps = sessionActivity.ActivitySets.Sum(x => x.PlannedNumOfReps ?? 0),
                TotalPlannedPracticeTime = sessionActivity.ActivitySets.Sum(x => x.PlannedPracticeTime ?? 0),
                TotalPlannedDistance = sessionActivity.ActivitySets.Sum(x => x.PlannedDistance ?? 0),
                IsCompleted = sessionActivity.ActivitySets.All(x => x.IsCompleted),
                AssetId = sessionActivity.AssetId,
                AssetType = sessionActivity.Asset != null ? sessionActivity.Asset.AssetType : null,
                AssetName = sessionActivity.Asset != null ? sessionActivity.Asset.Name : null,
                VietnameseAssetName = sessionActivity.Asset != null ? sessionActivity.Asset.VietNameseName : null,
                VietnameseAssetDescription = sessionActivity.Asset != null ? sessionActivity.Asset.VietnameseDescription : null,
                AssetImage = sessionActivity.Asset != null ? sessionActivity.Asset.MetadataImage : null,
            };
            sessionActivitiesDtos.Add(sessionActivityDto);
        }
        return new SessionPracticeContentDto {
            BookingId = request.BookingId,
            CustomerId = booking.CustomerId,
            SessionStartTime = sessionActivities.FirstOrDefault().Booking.SessionStartTime ?? null,
            SessionEndTime = sessionActivities.FirstOrDefault().Booking.SessionEndTime ?? null,
            SessionActivities = sessionActivitiesDtos,
            BookingName = sessionActivities.FirstOrDefault()?.Booking.BookingName,
            note = sessionActivities.FirstOrDefault()?.Booking.Note,
            NutritionTip = sessionActivities.FirstOrDefault()?.Booking.NutritionTip ?? "",
        };
    }
}
