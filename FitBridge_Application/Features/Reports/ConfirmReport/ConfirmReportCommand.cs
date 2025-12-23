using MediatR;
using System.Text.Json.Serialization;

namespace FitBridge_Application.Features.Reports.ConfirmReport
{
    public class ConfirmReportCommand : IRequest<ConfirmReportResponseDto>
    {
        [JsonIgnore]
        public Guid ReportId { get; set; }

        public string? Note { get; set; }
    }
}