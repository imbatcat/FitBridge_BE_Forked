using System;
using FitBridge_Application.Specifications.Bookings;
using FitBridge_Application.Specifications.Bookings.GetFreelancePtBookingForValidate;
using FitBridge_Domain.Entities.Trainings;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Exceptions;
using FitBridge_Application.Dtos.Bookings;

namespace FitBridge_Application.Services;

public class BookingService(IUnitOfWork _unitOfWork)
{
    public async Task<bool> ValidateBookingRequest(BookingRequest bookingRequest)
    {
        var bookingSpec = new GetBookingForValidationSpec(bookingRequest.CustomerId, bookingRequest.BookingDate, bookingRequest.StartTime, bookingRequest.EndTime);
        var booking = await _unitOfWork.Repository<Booking>().GetBySpecificationAsync(bookingSpec, false);
        if (booking != null)
        {
            throw new DuplicateException($"Người dùng đã có lịch tập tại thời gian này");
        }
        var freelancePtBookingSpec = new GetFreelancePtBookingForValidationSpec(bookingRequest.PtId, bookingRequest.BookingDate, bookingRequest.StartTime, bookingRequest.EndTime);
        var freelancePtBooking = await _unitOfWork.Repository<Booking>().GetBySpecificationAsync(freelancePtBookingSpec, false);
        if (freelancePtBooking != null)
        {
            throw new DuplicateException($"Người dùng đã có lịch tập tại thời gian này");
        }
        return true;
    }

    public async Task<bool> ValidateBookingRequestByDto(CreateRequestBookingDto requestBooking, Guid customerId, Guid ptId)
    {
        var bookingSpec = new GetBookingForValidationSpec(customerId, requestBooking.BookingDate, requestBooking.PtFreelanceStartTime, requestBooking.PtFreelanceEndTime);
        var booking = await _unitOfWork.Repository<Booking>().GetBySpecificationAsync(bookingSpec, false);
        if (booking != null)
        {
            throw new DuplicateException($"Người dùng đã có lịch tập tại thời gian này");
        }
        var freelancePtBookingSpec = new GetFreelancePtBookingForValidationSpec(ptId, requestBooking.BookingDate, requestBooking.PtFreelanceStartTime, requestBooking.PtFreelanceEndTime);
        var freelancePtBooking = await _unitOfWork.Repository<Booking>().GetBySpecificationAsync(freelancePtBookingSpec, false);
        if (freelancePtBooking != null)
        {
            throw new DuplicateException($"Người dùng đã có lịch tập tại thời gian này");
        }
        return true;
    }
}
