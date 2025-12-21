using System;
using MediatR;

namespace FitBridge_Application.Features.Payments.RevenueCatWebhook;

public class RevenueCatWebhookCommand : IRequest<bool>
{
    public string WebhookData { get; set; }
}



