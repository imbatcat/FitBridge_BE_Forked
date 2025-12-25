using System;
using System.Linq.Expressions;
using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Reports;
using FitBridge_Domain.Enums.Reports;

namespace FitBridge_Application.Specifications.Reports.GetReportSummary;

public class ReportByTypeSpecification : BaseSpecification<ReportCases>
{
    public ReportByTypeSpecification(ReportCaseType? reportType = null) 
        : base(reportType.HasValue ? x => x.ReportType == reportType.Value : x => true)
    {
    }
}
