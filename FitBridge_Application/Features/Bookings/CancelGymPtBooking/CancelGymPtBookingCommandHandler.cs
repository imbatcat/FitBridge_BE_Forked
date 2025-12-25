using System;
using FitBridge_Application.Interfaces.Repositories;
using MediatR;
using FitBridge_Domain.Entities.Trainings;
using FitBridge_Domain.Exceptions;
using FitBridge_Application.Commons.Constants;
using FitBridge_Domain.Enums.Trainings;
using FitBridge_Application.Services;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Dtos.Templates;
using FitBridge_Domain.Enums.MessageAndReview;
using FitBridge_Application.Dtos.Notifications;
using FitBridge_Application.Interfaces.Services.Notifications;

namespace FitBridge_Application.Features.Bookings.CancelGymPtBooking;

public class CancelGymPtBookingCommandHandler(IUnitOfWork _unitOfWork, SystemConfigurationService systemConfigurationService, IScheduleJobServices _scheduleJobServices, INotificationService notificationService) : IRequestHandler<CancelGymPtBookingCommand, bool>
{
    public async Task<bool> Handle(CancelGymPtBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(request.BookingId, false
        , includes: new List<string> { "PTGymSlot", "PTGymSlot.GymSlot", "CustomerPurchased" }
        );
        if (booking == null)
        {
            throw new NotFoundException("Booking not found");
        }
        var sessionDateTime = DateTime.UtcNow;
        if (booking.PTGymSlot != null)
        {
            sessionDateTime = booking.BookingDate.ToDateTime(booking.PTGymSlot.GymSlot.StartTime);
        }
        else
        {
            sessionDateTime = booking.BookingDate.ToDateTime(booking.PtFreelanceStartTime.Value);
        }
        if (sessionDateTime < DateTime.UtcNow)
        {
            throw new BusinessException("Cannot cancel a session that has already occurred");
        }

        var hoursUntilSession = sessionDateTime - DateTime.UtcNow;
        var defaultCancelBookingBeforeHours = (int)await systemConfigurationService.GetSystemConfigurationAutoConvertDataTypeAsync(ProjectConstant.SystemConfigurationKeys.CancelBookingBeforeHours);
        // Check cancellation policy and refund session if applicable
        if (hoursUntilSession.TotalHours > defaultCancelBookingBeforeHours)
        {
            if (booking.CustomerPurchased == null)
            {
                throw new InvalidOperationException("CustomerPurchased not found for booking");
            }
            booking.CustomerPurchased.AvailableSessions++;
            booking.IsSessionRefund = true;
        }
        await SendNotification(booking);
        booking.SessionStatus = SessionStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.CommitAsync();
        await _scheduleJobServices.CancelScheduleJob($"FinishedBookingSession_{booking.Id}", "FinishedBookingSession");

        return true;
    }
    
    private async Task SendNotification(Booking booking)
    {
        var sessionStartTime = booking.PTGymSlot.GymSlot.StartTime.ToString();
        if (booking.PTGymSlot != null)
        {
            sessionStartTime = booking.PtFreelanceStartTime.Value.ToString();
        }
        var model = new CancelBookingModel()
        {
            TitleBookingName = "Buổi tập bị hủy",
            BookingName = booking.BookingName ?? "",
            SessionStartTime = sessionStartTime,
            SessionDate = booking.BookingDate.ToString(),
        };
        var notificationMessage = new NotificationMessage(
            EnumContentType.BookingCancelled,
            new List<Guid> { booking.CustomerId, booking.PtId.Value },
            model);

        await notificationService.NotifyUsers(notificationMessage);
    }
}
