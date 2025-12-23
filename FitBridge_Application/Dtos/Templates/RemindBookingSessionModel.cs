using System;

namespace FitBridge_Application.Dtos.Templates;

public class RemindBookingSessionModel(string bookingName, string sessionStartTime, string sessionDate) : IBaseTemplateModel
{
    public string BookingName { get; set; } = bookingName;
    public string SessionStartTime { get; set; } = sessionStartTime;
    public string SessionDate { get; set; } = sessionDate;
}
