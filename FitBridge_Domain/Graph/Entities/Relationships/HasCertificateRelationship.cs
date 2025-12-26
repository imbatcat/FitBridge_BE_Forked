namespace FitBridge_Domain.Graph.Entities.Relationships
{
    public class HasCertificateRelationship : BaseRelationship
    {
        public string FreelancePTId { get; set; } = string.Empty;

        public string CertificateId { get; set; } = string.Empty;

        public string CertificateStatus { get; set; } = string.Empty;

        public string CertUrl { get; set; } = string.Empty;

        public DateOnly ProvidedDate { get; set; }

        public DateOnly ExpirationDate { get; set; }
    }
}