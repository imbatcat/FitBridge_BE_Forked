using System.Collections.Generic;
using FitBridge_Application.Dtos.Reports;

namespace FitBridge_Application.Dtos.Reports;

public class ReportPagingResultDto
{
    public int Total { get; set; }
    public IReadOnlyList<GetCustomerReportsResponseDto> Items { get; set; } = [];
    public ReportSummaryResponseDto Summary { get; set; } = new();
}
