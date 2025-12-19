using System;
using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Trainings;
using FitBridge_Domain.Enums.Trainings;

namespace FitBridge_Application.Specifications.Bookings.GetCustomerBookings;

public class GetCustomerBookingByCustomerIdSpecification : BaseSpecification<Booking>
{
    public GetCustomerBookingByCustomerIdSpecification(GetCustomerBookingsParams parameters) : base(x => x.CustomerId == parameters.CustomerId
    && x.IsEnabled
    && x.SessionStatus != SessionStatus.Cancelled
    && (parameters.Date == null || x.BookingDate == parameters.Date)
    && (parameters.CustomerPurchasedId == null || x.CustomerPurchasedId == parameters.CustomerPurchasedId))
    {
        AddInclude(x => x.PTGymSlot);
        AddInclude(x => x.PTGymSlot.GymSlot);
        AddInclude(x => x.PTGymSlot.PT);
        if (parameters.SortOrder == "desc" || parameters.SortOrder == "dsc")
        {
            AddOrderByDesc(x => x.BookingDate);
            AddThenBy(x => x.PtFreelanceStartTime);
        }
        else
        {
            AddOrderBy(x => x.BookingDate);
            AddThenBy(x => x.PtFreelanceStartTime);
        }
        if (parameters.DoApplyPaging)
        {
            AddPaging((parameters.Page - 1) * parameters.Size, parameters.Size);
        }
        else
        {
            parameters.Size = -1;
            parameters.Page = -1;
        }
    }
}