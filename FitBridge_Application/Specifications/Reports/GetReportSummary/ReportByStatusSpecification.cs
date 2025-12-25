using System;
using System.Linq.Expressions;
using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Reports;
using FitBridge_Domain.Enums.Reports;

namespace FitBridge_Application.Specifications.Reports.GetReportSummary;

public class ReportByStatusSpecification : BaseSpecification<ReportCases>
{
    public ReportByStatusSpecification(ReportCaseStatus? status = null) 
        : base(status.HasValue ? x => x.Status == status.Value : x => true)
    {
    }
}
