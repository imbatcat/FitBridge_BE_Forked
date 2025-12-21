using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitBridge_Application.Dtos.RevenueCat;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Services;
using FitBridge_Domain.Entities.ServicePackages;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.Payments.RevenueCatWebhook;

public class RevenueCatWebhookCommandHandler(
    IUnitOfWork _unitOfWork,
    SubscriptionService _subscriptionService,
    ILogger<RevenueCatWebhookCommandHandler> _logger
) : IRequestHandler<RevenueCatWebhookCommand, bool>
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<bool> Handle(RevenueCatWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if(request.WebhookData.Contains("TEST"))
            {
                _logger.LogInformation("Test webhook received");
                return true;
            }
            _logger.LogInformation("Received RevenueCat webhook: {WebhookData}", request.WebhookData);

            var webhook = JsonSerializer.Deserialize<RevenueCatWebhookDto>(request.WebhookData, _jsonOptions);
            if (webhook?.Event == null)
            {
                _logger.LogWarning("Invalid webhook data received");
                return false;
            }

            var eventData = webhook.Event;
            _logger.LogInformation(
                "Processing RevenueCat event: Type={EventType}, UserId={UserId}, ProductId={ProductId}, TransactionId={TransactionId}",
                eventData.Type,
                eventData.AppUserId,
                eventData.ProductId,
                eventData.TransactionId
            );

            // Route to appropriate handler based on event type
            var result = eventData.Type switch
            {
                "INITIAL_PURCHASE" => await _subscriptionService.HandleInitialPurchase(eventData),
                "RENEWAL" => await _subscriptionService.HandleRenewal(eventData),
                "CANCELLATION" => await _subscriptionService.HandleCancellation(eventData),
                "UNCANCELLATION" => await _subscriptionService.HandleUncancellation(eventData),
                "EXPIRATION" => await _subscriptionService.HandleExpiration(eventData),
                "BILLING_ISSUE" => await _subscriptionService.HandleBillingIssue(eventData),
                _ => HandleUnknownEventType(eventData.Type)
            };

            _logger.LogInformation("Successfully processed RevenueCat event: {EventType}", eventData.Type);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RevenueCat webhook: {Message}", ex.Message);
            throw;
        }
    }

    private bool HandleUnknownEventType(string eventType)
    {
        _logger.LogWarning("Unknown RevenueCat event type: {EventType}", eventType);
        // Return true to acknowledge receipt even if we don't process it
        return true;
    }
}



