using System;

namespace FitBridge_Application.Dtos.RevenueCat;

public class RevenueCatWebhookDto
{
    public string ApiVersion { get; set; }
    public RevenueCatEventDto Event { get; set; }
}



