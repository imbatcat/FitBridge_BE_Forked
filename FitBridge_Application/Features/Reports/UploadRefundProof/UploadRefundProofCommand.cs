using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FitBridge_Application.Features.Reports.UploadRefundProof
{
    public class UploadRefundProofCommand : IRequest
    {
        [Required]
        public Guid ReportId { get; set; }

        [Required]
        public IFormFile ResolvedEvidenceImage { get; set; }

        public string? Note { get; set; }
    }
}