using System;
using FitBridge_Application.Interfaces.Services;

namespace FitBridge_Infrastructure.Services;

public class ExchangeRateService : IExchangeRateService
{
    // Hardcoded exchange rates for now. In production, you would fetch these from an API.
    private readonly Dictionary<string, decimal> _exchangeRates = new()
    {
        { "VND", 1m },
        { "USD", 25000m },
        { "EUR", 27000m },
        { "GBP", 31000m },
        { "JPY", 170m },
        { "CNY", 3500m },
        { "KRW", 19m },
        { "THB", 750m },
        { "SGD", 18500m },
        { "AUD", 16000m },
        { "CAD", 17500m },
        { "CHF", 28000m },
        { "HKD", 3200m },
        { "NZD", 15000m },
        { "SEK", 2300m },
        { "MXN", 1400m },
        { "NOK", 2300m },
        { "DKK", 3600m },
        { "PLN", 6200m },
        { "RUB", 260m },
        { "PHP", 440m }
    };

    public async Task<decimal> ConvertToVnd(decimal amount, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException("Currency code cannot be null or empty", nameof(currencyCode));
        }

        var upperCurrency = currencyCode.ToUpper();

        if (upperCurrency == "VND")
        {
            return amount;
        }

        if (!_exchangeRates.TryGetValue(upperCurrency, out var rate))
        {
            // Default to USD rate if currency is not found
            rate = _exchangeRates["USD"];
        }

        var convertedAmount = Math.Round(amount * rate, 0, MidpointRounding.AwayFromZero);
        return convertedAmount;
    }
}



