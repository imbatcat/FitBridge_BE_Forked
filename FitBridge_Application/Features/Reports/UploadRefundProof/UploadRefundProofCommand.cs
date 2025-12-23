using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace FitBridge_Application.Features.Reports.UploadRefundProof
{
    public class UploadRefundProofCommand : IRequest
    {
        [JsonIgnore]
        public Guid ReportId { get; set; }

        public IFormFile ResolvedEvidenceImage { get; set; }

        public string? Note { get; set; }
    }
}