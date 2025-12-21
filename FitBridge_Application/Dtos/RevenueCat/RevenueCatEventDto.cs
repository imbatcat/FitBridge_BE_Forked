using System;

namespace FitBridge_Application.Dtos.RevenueCat;

public class RevenueCatEventDto
{
    public List<string> Aliases { get; set; }
    public string AppId { get; set; }
    public string AppUserId { get; set; }
    public decimal? CommissionPercentage { get; set; }
    public string CountryCode { get; set; }
    public string Currency { get; set; }
    public string? EntitlementId { get; set; }
    public List<string> EntitlementIds { get; set; }
    public string Environment { get; set; }
    public long EventTimestampMs { get; set; }
    public long? ExpirationAtMs { get; set; }
    public string Id { get; set; }
    public bool? IsFamilyShare { get; set; }
    public bool? IsTrialConversion { get; set; }
    public object? Metadata { get; set; }
    public string? OfferCode { get; set; }
    public string OriginalAppUserId { get; set; }
    public string? OriginalTransactionId { get; set; }
    public string PeriodType { get; set; }
    public string? PresentedOfferingId { get; set; }
    public decimal? Price { get; set; }
    public decimal? PriceInPurchasedCurrency { get; set; }
    public string ProductId { get; set; }
    public long? PurchasedAtMs { get; set; }
    public int? RenewalNumber { get; set; }
    public string Store { get; set; }
    public Dictionary<string, SubscriberAttributeDto> SubscriberAttributes { get; set; }
    public decimal? TakehomePercentage { get; set; }
    public decimal? TaxPercentage { get; set; }
    public string? TransactionId { get; set; }
    public string Type { get; set; }
    public string? CancelReason { get; set; }
    public string? ExpirationReason { get; set; }
    public List<ExperimentDto>? Experiments { get; set; }
}


