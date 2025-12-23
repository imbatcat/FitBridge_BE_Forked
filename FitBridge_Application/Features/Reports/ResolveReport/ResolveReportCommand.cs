using MediatR;
using System.Text.Json.Serialization;

namespace FitBridge_Application.Features.Reports.ResolveReport
{
    public class ResolveReportCommand : IRequest
    {
        [JsonIgnore]
        public Guid ReportId { get; set; }

        public string? Note { get; set; }
    }
}
