using System;
using FitBridge_Application.Specifications;

namespace FitBridge_Application.Features.Dashboards.GetDashboardFinancialStats;

public class GetDashboardFinancialStatsParams : BaseParams
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

