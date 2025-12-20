using System;
using MediatR;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Exceptions;
using FitBridge_Application.Interfaces.Utils;
using Microsoft.AspNetCore.Http;
using FitBridge_Application.Specifications.Bookings;
using FitBridge_Domain.Entities.Trainings;
using FitBridge_Domain.Enums.Trainings;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Dtos.Jobs;

namespace FitBridge_Application.Features.GymSlots.CustomerRegisterSlot;

public class CustomerRegisterSlotCommandHandler(IUnitOfWork _unitOfWork, IUserUtil _userUtil, IHttpContextAccessor _httpContextAccessor, IScheduleJobServices _scheduleJobServices) : IRequestHandler<CustomerRegisterSlotCommand, bool>
{
    public async Task<bool> Handle(CustomerRegisterSlotCommand request, CancellationToken cancellationToken)
    {
        var userId = _userUtil.GetAccountId(_httpContextAccessor.HttpContext);
        if (userId == null)
        {
            throw new NotFoundException("User not found");
        }
        var ptGymSlot = await _unitOfWork.Repository<PTGymSlot>().GetByIdAsync(request.PtGymSlotId, false, new List<string> { "Booking", "GymSlot" });
        if (ptGymSlot == null || ptGymSlot.Booking != null)
        {
            throw new DuplicateException("Slot already booked by another customer");
        }

        var bookingSpec = new GetBookingForValidationSpec(userId.Value, ptGymSlot.RegisterDate, ptGymSlot.GymSlot.StartTime, ptGymSlot.GymSlot.EndTime);
        var booking = await _unitOfWork.Repository<Booking>().GetBySpecificationAsync(bookingSpec, false);
        if (booking != null)
        {
            throw new DuplicateException("Slot overlapped by freelance pt course");
        }
        var customerPurchased = await _unitOfWork.Repository<CustomerPurchased>().GetByIdAsync(request.CustomerPurchasedId, false, new List<string> { "OrderItems" });
        if (customerPurchased == null)
        {
            throw new NotFoundException("Customer purchased not found");
        }
        if (customerPurchased.AvailableSessions <= 0)
        {
            throw new NotEnoughSessionException("Customer purchased not enough sessions");
        }
        customerPurchased.AvailableSessions--;
        var insertBooking = new Booking
        {
            BookingName = ptGymSlot.GymSlot.Name,
            CustomerId = userId.Value,
            CustomerPurchasedId = request.CustomerPurchasedId,
            PTGymSlotId = request.PtGymSlotId,
            PtId = ptGymSlot.PTId,
            BookingDate = ptGymSlot.RegisterDate,
            SessionStatus = SessionStatus.Booked,
        };
        _unitOfWork.Repository<Booking>().Insert(insertBooking);
        await _unitOfWork.CommitAsync();
        await _scheduleJobServices.ScheduleFinishedBookingSession(new FinishedBookingSessionJobScheduleDto
        {
            BookingId = insertBooking.Id,
            TriggerTime = ptGymSlot.RegisterDate.ToDateTime(ptGymSlot.GymSlot.EndTime, DateTimeKind.Utc)
        });
        return true;
    }
}
