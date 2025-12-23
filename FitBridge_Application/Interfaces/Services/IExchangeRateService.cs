using System;

namespace FitBridge_Application.Interfaces.Services;

public interface IExchangeRateService
{
    Task<decimal> ConvertToVnd(decimal amount, string currencyCode);
}



