using System;
using FitBridge_Application.Dtos.Dashboard;
using MediatR;

namespace FitBridge_Application.Features.Dashboards.GetDashboardFinancialStats;

public class GetDashboardFinancialStatsQuery : IRequest<DashboardStatsDto>
{
    public required GetDashboardFinancialStatsParams Params { get; set; }
}

