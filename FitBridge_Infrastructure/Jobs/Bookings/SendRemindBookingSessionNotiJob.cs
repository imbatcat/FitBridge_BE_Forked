using System;
using FitBridge_Domain.Exceptions;
using Quartz;
using Microsoft.Extensions.Logging;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Trainings;
using FitBridge_Domain.Enums.MessageAndReview;
using FitBridge_Application.Dtos.Notifications;
using FitBridge_Application.Interfaces.Services.Notifications;
using FitBridge_Application.Dtos.Templates;
using FitBridge_Domain.Enums.Trainings;

namespace FitBridge_Infrastructure.Jobs.Bookings;

public class SendRemindBookingSessionNotiJob(ILogger<SendRemindBookingSessionNotiJob> _logger, IUnitOfWork _unitOfWork, INotificationService _notificationService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var bookingId = Guid.Parse(context.JobDetail.JobDataMap.GetString("bookingId")
            ?? throw new NotFoundException($"{nameof(SendRemindBookingSessionNotiJob)}_bookingId"));
        _logger.LogInformation("SendRemindBookingSessionNotiJob started for Booking: {BookingId}", bookingId);
        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(bookingId, includes: new List<string> { "PTGymSlot", "PTGymSlot.GymSlot" });
        if (booking == null)
        {
            _logger.LogError("Booking not found for BookingId: {BookingId}", bookingId);
            return;
        }
        var sessionStartTime = booking.PTGymSlot.GymSlot.StartTime.ToString();
        if (booking.PTGymSlot != null)
        {
            sessionStartTime = booking.PtFreelanceStartTime.Value.ToString();
        }
        if(booking.SessionStatus == SessionStatus.Cancelled)
        {
            _logger.LogError("Booking is cancelled, current status: {SessionStatus}", booking.SessionStatus);
            return;
        }
        var model = new RemindBookingSessionModel(booking.BookingName ?? "", sessionStartTime, booking.BookingDate.ToString());
        var notificationMessage = new NotificationMessage(EnumContentType.RemindBookingSession, new List<Guid> { booking.CustomerId, booking.PtId.Value }, model);
        await _notificationService.NotifyUsers(notificationMessage);
    }
}
