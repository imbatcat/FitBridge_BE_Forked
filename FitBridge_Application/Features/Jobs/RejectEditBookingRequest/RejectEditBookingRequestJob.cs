using System;
using Quartz;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Trainings;
using FitBridge_Domain.Exceptions;
using FitBridge_Domain.Enums.Trainings;

namespace FitBridge_Application.Features.Jobs.RejectEditBookingRequest;

public class RejectEditBookingRequestJob(IUnitOfWork _unitOfWork) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var bookingRequestId = context.JobDetail.JobDataMap.GetString("bookingRequestId");
        var bookingRequest = await _unitOfWork.Repository<BookingRequest>().GetByIdAsync(Guid.Parse(bookingRequestId), includes: new List<string> { "TargetBooking" });
        if (bookingRequest == null)
        {
            throw new NotFoundException("Booking request not found");
        }
        bookingRequest.RequestStatus = BookingRequestStatus.Rejected;
        bookingRequest.UpdatedAt = DateTime.UtcNow;
        bookingRequest.TargetBooking.SessionStatus = SessionStatus.Booked;
        _unitOfWork.Repository<Booking>().Update(bookingRequest.TargetBooking);
        _unitOfWork.Repository<BookingRequest>().Update(bookingRequest);
        await _unitOfWork.CommitAsync();
    }
}
