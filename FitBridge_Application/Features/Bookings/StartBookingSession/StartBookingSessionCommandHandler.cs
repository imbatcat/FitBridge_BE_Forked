using System;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Trainings;
using FitBridge_Domain.Exceptions;
using FitBridge_Domain.Enums.Trainings;
using MediatR;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Dtos.Jobs;
using FitBridge_Application.Services;
using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Specifications.Bookings.GetDuplicateStartBookingSession;

namespace FitBridge_Application.Features.Bookings.StartBookingSession;

public class StartBookingSessionCommandHandler(IUnitOfWork _unitOfWork, IScheduleJobServices _scheduleJobServices, SystemConfigurationService systemConfigurationService) : IRequestHandler<StartBookingSessionCommand, DateTime>
{
    public async Task<DateTime> Handle(StartBookingSessionCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(request.BookingId, includes: new List<string> { nameof(Booking.CustomerPurchased), "CustomerPurchased.OrderItems", "CustomerPurchased.OrderItems.FreelancePTPackage", "SessionActivities" });
        if (booking == null)
        {
            throw new NotFoundException("Booking not found");
        }
        if (booking.SessionStartTime != null)
        {
            throw new BusinessException("Booking session already started");
        }
        if(booking.SessionActivities == null || booking.SessionActivities.Count == 0)
        {
            throw new BusinessException("Không thể bắt đầu buổi tập không có hoạt động, vui lòng liên hệ huấn luyện viên của bạn");
        }
        var currentDate = DateTime.UtcNow;
        await CheckSessionStart(booking);
        var earlyStartSessionBeforeMinutes = (int)await systemConfigurationService.GetSystemConfigurationAutoConvertDataTypeAsync(ProjectConstant.SystemConfigurationKeys.EarlyStartSessionBeforeMinutes);
        var earliestStartSessionTime = booking.BookingDate.ToDateTime(booking.PtFreelanceStartTime.Value, DateTimeKind.Utc).AddMinutes(-earlyStartSessionBeforeMinutes);
        if (earliestStartSessionTime > currentDate)
        {
            throw new BusinessException($"Không thể bắt đầu sớm trước thời gian bắt đầu buổi tập quá {earlyStartSessionBeforeMinutes} phút");
        }
        booking.SessionStartTime = currentDate;
        booking.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Booking>().Update(booking);
        await ScheduleFinishedBookingSession(booking);
        await CancelAutoCancelBookingJob(booking);
        await _unitOfWork.CommitAsync();
        return booking.SessionStartTime.Value;
    }

    public async Task ScheduleFinishedBookingSession(Booking booking)
    {
        var durationInMinutes = booking.CustomerPurchased.OrderItems.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x => x.FreelancePTPackageId != null)?.FreelancePTPackage?.SessionDurationInMinutes ?? 0;
        await _scheduleJobServices.ScheduleFinishedBookingSession(new FinishedBookingSessionJobScheduleDto
        {
            BookingId = booking.Id,
            TriggerTime = booking.SessionStartTime.Value.AddMinutes(durationInMinutes)
        });
    }

    public async Task CancelAutoCancelBookingJob(Booking booking)
    {
        await _scheduleJobServices.CancelScheduleJob($"AutoCancelBooking_{booking.Id}", "AutoCancelBooking");
    }

    public async Task CheckSessionStart(Booking booking)
    {
        var concurrentBookingSpec = new GetDuplicateStartBookingSessionSpec(booking.Id, booking.CustomerId);
        var concurrentBooking = await _unitOfWork.Repository<Booking>().GetBySpecificationAsync(concurrentBookingSpec);
        if (concurrentBooking != null)
        {
            throw new BusinessException($"Khách hàng đã có buổi tập khác được bắt đầu vào lúc {concurrentBooking.PtFreelanceStartTime}, ngày {concurrentBooking.BookingDate} và vẫn chưa kết thúc");
        }
    }
}