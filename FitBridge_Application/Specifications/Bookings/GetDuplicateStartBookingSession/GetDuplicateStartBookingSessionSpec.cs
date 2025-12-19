using System;
using FitBridge_Domain.Entities.Trainings;
using FitBridge_Domain.Enums.Trainings;

namespace FitBridge_Application.Specifications.Bookings.GetDuplicateStartBookingSession;

public class GetDuplicateStartBookingSessionSpec : BaseSpecification<Booking>
{
    public GetDuplicateStartBookingSessionSpec(Guid bookingId, Guid customerId)
        : base(x => x.IsEnabled
            && x.Id != bookingId
            && x.CustomerId == customerId
            && x.SessionStartTime != null
            && x.SessionEndTime == null
            && x.SessionStatus != SessionStatus.Cancelled
            && x.SessionStatus != SessionStatus.Finished)
    {
    }
}
