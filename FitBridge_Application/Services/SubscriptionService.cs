using System;
using System.Security.Cryptography.X509Certificates;
using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos.RevenueCat;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.PaymentMethods;
using FitBridge_Application.Specifications.SubscriptionPlans;
using FitBridge_Application.Specifications.Subscriptions.GetHotResearchSubscription;
using FitBridge_Application.Specifications.Subscriptions.GetUserSubscriptionByOriginalTransactionId;
using FitBridge_Application.Specifications.Transactions;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Entities.ServicePackages;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Enums.SubscriptionPlans;
using FitBridge_Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Services;

public class SubscriptionService(
    IUnitOfWork _unitOfWork, 
    IApplicationUserService _applicationUserService, 
    SystemConfigurationService _systemConfigurationService,
    IExchangeRateService _exchangeRateService,
    ILogger<SubscriptionService> _logger,
    IPayOSService _payOSService)
{
    public async Task<bool> ExpireUserSubscription(Guid userSubscriptionId)
    {
        var userSubscription = await _unitOfWork.Repository<UserSubscription>().GetByIdAsync(userSubscriptionId, false, includes: new List<string> { "User", "SubscriptionPlansInformation", "SubscriptionPlansInformation.FeatureKey" });
        if (userSubscription == null)
        {
            throw new NotFoundException($"User subscription not found for user subscription {userSubscriptionId}");
        }
        userSubscription.Status = SubScriptionStatus.Expired;
        userSubscription.UpdatedAt = DateTime.UtcNow;
        var featureKey = userSubscription.SubscriptionPlansInformation.FeatureKey;
        if (featureKey.FeatureName == ProjectConstant.FeatureKeyNames.HotResearch)
        {
            await RevokeHotResearchSubscriptionPlanBenefit(userSubscription.User);
        }
        await _unitOfWork.CommitAsync();
        return true;
    }

    public async Task RevokeHotResearchSubscriptionPlanBenefit(ApplicationUser user)
    {
        user.hotResearch = false;
        user.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<int> GetNumOfCurrentHotResearchSubscription()
    {
        var numOfHotResearchSubscription = await _unitOfWork.Repository<UserSubscription>().CountAsync(new GetHotResearchSubscriptionSpecification());
        return numOfHotResearchSubscription;
    }

    #region RevenueCat Webhook Handlers

    public async Task<bool> HandleInitialPurchase(RevenueCatEventDto eventData)
    {
        _logger.LogInformation("Handling INITIAL_PURCHASE for user {UserId}, product {ProductId}", 
            eventData.AppUserId, eventData.ProductId);

        // Check for idempotency - ensure we don't duplicate
        if (!string.IsNullOrWhiteSpace(eventData.TransactionId))
        {
            var existingTransaction = await CheckTransactionExists(eventData.TransactionId);
            if (existingTransaction)
            {
                _logger.LogWarning("Transaction {TransactionId} already processed. Skipping.", eventData.TransactionId);
                return true;
            }
        }

        // Find the subscription plan by InAppPurchaseId
        var subscriptionPlan = await FindSubscriptionPlanByProductId(eventData.ProductId);
        if (subscriptionPlan == null)
        {
            _logger.LogError("Subscription plan not found for product {ProductId}", eventData.ProductId);
            throw new NotFoundException($"Subscription plan not found for product {eventData.ProductId}");
        }

        // Find or create user
        var user = await FindUserByAppUserId(eventData.AppUserId);
        if (user == null)
        {
            _logger.LogError("User not found for AppUserId {AppUserId}", eventData.AppUserId);
            throw new NotFoundException($"User not found for AppUserId {eventData.AppUserId}");
        }
        var priceInPurchasedCurrency = eventData.PriceInPurchasedCurrency ?? 0;
        var takehomePercentage = eventData.TakehomePercentage ?? 0;
        // Convert currency to VND
        var amountInVnd = await _exchangeRateService.ConvertToVnd(
            priceInPurchasedCurrency, 
            eventData.Currency);
        var profitInVnd = await _exchangeRateService.ConvertToVnd(
            priceInPurchasedCurrency * takehomePercentage,
            eventData.Currency);

        // Calculate subscription end date
        var startDate = DateTime.UtcNow;
        var endDate = CalculateSubscriptionEndDate(startDate, subscriptionPlan.Duration);

        // Create UserSubscription
        var userSubscription = new UserSubscription
        {
            UserId = user.Id,
            SubscriptionPlanId = subscriptionPlan.Id,
            OriginalTransactionId = eventData.OriginalTransactionId,
            StartDate = startDate,
            EndDate = endDate,
            Status = SubScriptionStatus.Active,
            LimitUsage = subscriptionPlan.LimitUsage,
            CurrentUsage = 0,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<UserSubscription>().Insert(userSubscription);

        // Apply subscription benefits
        await ApplySubscriptionBenefits(user, subscriptionPlan);

        // Create Order
        var order = new Order
        {
            AccountId = user.Id,
            Status = OrderStatus.Finished,
            CheckoutUrl = string.Empty,
            SubTotalPrice = amountInVnd,
            TotalAmount = amountInVnd,
            ShippingFee = 0,
            CommissionRate = 0,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<Order>().Insert(order);

        // Create OrderItem
        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            UserSubscriptionId = userSubscription.Id,
            Quantity = 1,
            Price = amountInVnd,
            IsFeedback = false,
            IsRefunded = false,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<OrderItem>().Insert(orderItem);

        // Find RevenueCat payment method (or create a default one)
        var paymentMethod = await _unitOfWork.Repository<PaymentMethod>().GetBySpecificationAsync(new GetPaymentMethodByTypeSpecification(MethodType.InAppPurchase));

        // Create Transaction
        var transaction = new Transaction
        {
            OrderCode = long.Parse(eventData.TransactionId ?? _payOSService.GenerateOrderCode().ToString()),
            Description = $"RevenueCat Initial Purchase - {eventData.ProductId} - TransactionId: {eventData.TransactionId}",
            PaymentMethodId = paymentMethod.Id,
            TransactionType = TransactionType.SubscriptionPlansOrder,
            Status = TransactionStatus.Success,
            OrderId = order.Id,
            Amount = amountInVnd,
            ProfitAmount = profitInVnd,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<Transaction>().Insert(transaction);

        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Successfully processed INITIAL_PURCHASE for subscription {SubscriptionId}", 
            userSubscription.Id);
        return true;
    }

    public async Task<bool> HandleRenewal(RevenueCatEventDto eventData)
    {
        _logger.LogInformation("Handling RENEWAL for user {UserId}, original transaction {OriginalTransactionId}", 
            eventData.AppUserId, eventData.OriginalTransactionId);

        // Check for idempotency
        if (!string.IsNullOrWhiteSpace(eventData.TransactionId))
        {
            var existingTransaction = await CheckTransactionExists(eventData.TransactionId);
            if (existingTransaction)
            {
                _logger.LogWarning("Transaction {TransactionId} already processed. Skipping.", eventData.TransactionId);
                return true;
            }
        }

        // Find existing subscription by OriginalTransactionId
        if (string.IsNullOrWhiteSpace(eventData.OriginalTransactionId))
        {
            _logger.LogError("OriginalTransactionId is null or empty in RENEWAL event");
            throw new BusinessException("OriginalTransactionId is required for renewal events");
        }

        var userSubscription = await FindUserSubscriptionByOriginalTransactionId(eventData.OriginalTransactionId);
        if (userSubscription == null)
        {
            _logger.LogError("User subscription not found for original transaction {OriginalTransactionId}", 
                eventData.OriginalTransactionId);
            throw new NotFoundException($"User subscription not found for original transaction {eventData.OriginalTransactionId}");
        }
        var priceInPurchasedCurrency = eventData.PriceInPurchasedCurrency ?? 0;
        var takehomePercentage = eventData.TakehomePercentage ?? 0;
        // Convert currency to VND
        var amountInVnd = await _exchangeRateService.ConvertToVnd(
            priceInPurchasedCurrency, 
            eventData.Currency);
        var profitInVnd = await _exchangeRateService.ConvertToVnd(
            priceInPurchasedCurrency * takehomePercentage,
            eventData.Currency);

        // Extend subscription
        if (!eventData.ExpirationAtMs.HasValue)
        {
            _logger.LogError("ExpirationAtMs is null in RENEWAL event");
            throw new BusinessException("ExpirationAtMs is required for renewal events");
        }
        
        var newEndDate = DateTimeOffset.FromUnixTimeMilliseconds(eventData.ExpirationAtMs.Value).UtcDateTime;
        userSubscription.EndDate = newEndDate;
        userSubscription.Status = SubScriptionStatus.Active;
        userSubscription.UpdatedAt = DateTime.UtcNow;

        // Apply subscription benefits (in case they were revoked)
        await ApplySubscriptionBenefits(userSubscription.User, userSubscription.SubscriptionPlansInformation);

        // Create Order for renewal
        var order = new Order
        {
            AccountId = userSubscription.UserId,
            Status = OrderStatus.Finished,
            CheckoutUrl = string.Empty,
            SubTotalPrice = amountInVnd,
            TotalAmount = amountInVnd,
            ShippingFee = 0,
            CommissionRate = 0,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<Order>().Insert(order);

        // Create OrderItem linked to existing subscription
        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            UserSubscriptionId = userSubscription.Id,
            Quantity = 1,
            Price = amountInVnd,
            IsFeedback = false,
            IsRefunded = false,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<OrderItem>().Insert(orderItem);

        // Find RevenueCat payment method
        var paymentMethod = await _unitOfWork.Repository<PaymentMethod>().GetBySpecificationAsync(new GetPaymentMethodByTypeSpecification(MethodType.InAppPurchase));

        // Create Transaction
        var transaction = new Transaction
        {
            OrderCode = long.Parse(eventData.TransactionId ?? _payOSService.GenerateOrderCode().ToString()),
            Description = $"RevenueCat Renewal - {eventData.ProductId} - TransactionId: {eventData.TransactionId}",
            PaymentMethodId = paymentMethod.Id,
            TransactionType = TransactionType.SubscriptionPlansOrder,
            Status = TransactionStatus.Success,
            OrderId = order.Id,
            Amount = amountInVnd,
            ProfitAmount = profitInVnd,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<Transaction>().Insert(transaction);

        _unitOfWork.Repository<UserSubscription>().Update(userSubscription);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Successfully processed RENEWAL for subscription {SubscriptionId}", 
            userSubscription.Id);
        return true;
    }

    public async Task<bool> HandleCancellation(RevenueCatEventDto eventData)
    {
        _logger.LogInformation("Handling CANCELLATION for original transaction {OriginalTransactionId}", 
            eventData.OriginalTransactionId);

        if (string.IsNullOrWhiteSpace(eventData.OriginalTransactionId))
        {
            _logger.LogWarning("OriginalTransactionId is null or empty in CANCELLATION event");
            return true;
        }

        var userSubscription = await FindUserSubscriptionByOriginalTransactionId(eventData.OriginalTransactionId);
        if (userSubscription == null)
        {
            _logger.LogWarning("User subscription not found for original transaction {OriginalTransactionId}", 
                eventData.OriginalTransactionId);
            return true; // Return true to acknowledge
        }

        // Update status to Active_Not_Renewed (access remains until EndDate)
        userSubscription.Status = SubScriptionStatus.Active_Not_Renewed;
        userSubscription.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<UserSubscription>().Update(userSubscription);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Successfully processed CANCELLATION for subscription {SubscriptionId}. Status set to Active_Not_Renewed", 
            userSubscription.Id);
        return true;
    }

    public async Task<bool> HandleUncancellation(RevenueCatEventDto eventData)
    {
        _logger.LogInformation("Handling UNCANCELLATION for original transaction {OriginalTransactionId}", 
            eventData.OriginalTransactionId);

        if (string.IsNullOrWhiteSpace(eventData.OriginalTransactionId))
        {
            _logger.LogWarning("OriginalTransactionId is null or empty in UNCANCELLATION event");
            return true;
        }

        var userSubscription = await FindUserSubscriptionByOriginalTransactionId(eventData.OriginalTransactionId);
        if (userSubscription == null)
        {
            _logger.LogWarning("User subscription not found for original transaction {OriginalTransactionId}", 
                eventData.OriginalTransactionId);
            return true;
        }

        // Re-enable auto-renewal
        userSubscription.Status = SubScriptionStatus.Active;
        userSubscription.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<UserSubscription>().Update(userSubscription);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Successfully processed UNCANCELLATION for subscription {SubscriptionId}", 
            userSubscription.Id);
        return true;
    }

    public async Task<bool> HandleExpiration(RevenueCatEventDto eventData)
    {
        _logger.LogInformation("Handling EXPIRATION for original transaction {OriginalTransactionId}", 
            eventData.OriginalTransactionId);

        if (string.IsNullOrWhiteSpace(eventData.OriginalTransactionId))
        {
            _logger.LogWarning("OriginalTransactionId is null or empty in EXPIRATION event");
            return true;
        }

        var userSubscription = await FindUserSubscriptionByOriginalTransactionId(eventData.OriginalTransactionId);
        if (userSubscription == null)
        {
            _logger.LogWarning("User subscription not found for original transaction {OriginalTransactionId}", 
                eventData.OriginalTransactionId);
            return true;
        }

        // Mark as expired
        userSubscription.Status = SubScriptionStatus.Expired;
        userSubscription.UpdatedAt = DateTime.UtcNow;

        // Revoke subscription benefits
        await RevokeSubscriptionBenefits(userSubscription.User, userSubscription.SubscriptionPlansInformation);
        _unitOfWork.Repository<UserSubscription>().Update(userSubscription);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Successfully processed EXPIRATION for subscription {SubscriptionId}", 
            userSubscription.Id);
        return true;
    }

    public async Task<bool> HandleBillingIssue(RevenueCatEventDto eventData)
    {
        _logger.LogInformation("Handling BILLING_ISSUE for original transaction {OriginalTransactionId}", 
            eventData.OriginalTransactionId);

        if (string.IsNullOrWhiteSpace(eventData.OriginalTransactionId))
        {
            _logger.LogWarning("OriginalTransactionId is null or empty in BILLING_ISSUE event");
            return true;
        }

        var userSubscription = await FindUserSubscriptionByOriginalTransactionId(eventData.OriginalTransactionId);
        if (userSubscription == null)
        {
            _logger.LogWarning("User subscription not found for original transaction {OriginalTransactionId}", 
                eventData.OriginalTransactionId);
            return true;
        }

        // Immediately revoke access
        userSubscription.Status = SubScriptionStatus.Cancelled;
        userSubscription.UpdatedAt = DateTime.UtcNow;

        // Revoke subscription benefits
        await RevokeSubscriptionBenefits(userSubscription.User, userSubscription.SubscriptionPlansInformation);
        _unitOfWork.Repository<UserSubscription>().Update(userSubscription);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Successfully processed BILLING_ISSUE for subscription {SubscriptionId}. Access revoked.", 
            userSubscription.Id);
        return true;
    }

    #endregion

    #region Helper Methods

    private async Task<bool> CheckTransactionExists(string? transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return false;
        }

        var spec = new GetTransactionByDescriptionSpecification(transactionId);
        return await _unitOfWork.Repository<Transaction>().AnyAsync(spec);
    }

    private async Task<SubscriptionPlansInformation?> FindSubscriptionPlanByProductId(string productId)
    {
        var spec = new GetSubscriptionPlanByInAppPurchaseIdSpecification(productId);
        return await _unitOfWork.Repository<SubscriptionPlansInformation>().GetBySpecificationAsync(spec);
    }

    private async Task<ApplicationUser?> FindUserByAppUserId(string appUserId)
    {
        // Try to parse as Guid first
        if (Guid.TryParse(appUserId, out var userId))
        {
            return await _applicationUserService.GetByIdAsync(userId);
        }

        // If not a Guid, search by email
        return await _applicationUserService.GetUserByEmailAsync(appUserId);
    }

    private async Task<UserSubscription?> FindUserSubscriptionByOriginalTransactionId(string originalTransactionId)
    {
        var spec = new GetUserSubscriptionByOriginalTransactionIdSpecification(originalTransactionId);
        return await _unitOfWork.Repository<UserSubscription>().GetBySpecificationAsync(spec);
    }

    private DateTime CalculateSubscriptionEndDate(DateTime startDate, int durationInDays)
    {
        return startDate.AddDays(durationInDays);
    }

    private async Task ApplySubscriptionBenefits(ApplicationUser user, SubscriptionPlansInformation subscriptionPlan)
    {
        if (subscriptionPlan.FeatureKey?.FeatureName == ProjectConstant.FeatureKeyNames.HotResearch)
        {
            user.hotResearch = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _applicationUserService.UpdateAsync(user);
        }
    }

    private async Task RevokeSubscriptionBenefits(ApplicationUser user, SubscriptionPlansInformation subscriptionPlan)
    {
        if (subscriptionPlan.FeatureKey?.FeatureName == ProjectConstant.FeatureKeyNames.HotResearch)
        {
            await RevokeHotResearchSubscriptionPlanBenefit(user);
        }
    }
    #endregion
}
