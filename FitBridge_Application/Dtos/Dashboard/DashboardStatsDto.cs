using System;

namespace FitBridge_Application.Dtos.Dashboard;

public class DashboardStatsDto
{
    public SummaryCardsDto SummaryCards { get; set; }
    public List<ProfitDistributionDto> ProfitDistributionChart { get; set; }
    // public List<ChartPointDto> RevenueProfitOverTimeChart { get; set; }
    public PagingResultDto<RecentTransactionDto> RecentTransactionsTable { get; set; }
}

