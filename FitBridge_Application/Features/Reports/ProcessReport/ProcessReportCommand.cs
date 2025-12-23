using MediatR;
using System.Text.Json.Serialization;

namespace FitBridge_Application.Features.Reports.ProcessReport
{
    public class ProcessReportCommand : IRequest
    {
        [JsonIgnore]
        public Guid ReportId { get; set; }
    }
}
