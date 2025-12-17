using System;
using System.ComponentModel.DataAnnotations;

namespace FitBridge_Application.Specifications.Bookings.GetCustomerBookings;

public class GetCustomerBookingsParams : BaseParams
{
    public Guid CustomerId { get; set; }
    public DateOnly? Date { get; set; }
    public Guid? CustomerPurchasedId { get; set; }
}
