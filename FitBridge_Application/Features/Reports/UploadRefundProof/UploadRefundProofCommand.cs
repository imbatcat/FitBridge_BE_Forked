using MediatR;
using System.Text.Json.Serialization;

namespace FitBridge_Application.Features.Reports.UploadRefundProof
{
    public class UploadRefundProofCommand : IRequest
    {
        [JsonIgnore]
        public Guid ReportId { get; set; }

        public string ResolvedEvidenceImageUrl { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}