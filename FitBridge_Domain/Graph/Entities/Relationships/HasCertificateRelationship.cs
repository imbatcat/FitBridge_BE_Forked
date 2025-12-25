namespace FitBridge_Domain.Graph.Entities.Relationships
{
    public class HasCertificateRelationship
    {
        public string FreelancePTId { get; set; } = string.Empty;

        public string CertificateId { get; set; } = string.Empty;

        public string CertificateStatus { get; set; } = string.Empty;

        public string CertUrl { get; set; } = string.Empty;

        public DateTime ProvidedDate { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}