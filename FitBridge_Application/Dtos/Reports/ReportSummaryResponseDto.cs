using System;

namespace FitBridge_Application.Dtos.Reports;

public class ReportSummaryResponseDto
{
    public int TotalReports { get; set; }
    public int ProductReportCount { get; set; }
    public int FreelancePtReportCount { get; set; }
    public int GymCourseReportCount { get; set; }
    public int PendingCount { get; set; }
    public int ProcessingCount { get; set; }
    public int ResolvedCount { get; set; }
    public int FraudConfirmedCount { get; set; }
}
