using System;

namespace FitBridge_Application.Dtos.Templates;

public class CancelBookingModel : IBaseTemplateModel
{
    public string TitleBookingName { get; set; }
    public string BookingName { get; set; }
    public string SessionStartTime { get; set; }
    public string SessionDate { get; set; }
}
