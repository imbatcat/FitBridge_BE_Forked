namespace FitBridge_Domain.Graph.Entities
{
    public class CertificateNode : BaseNode
    {
        public string DbId { get; set; } = string.Empty;

        public string CertCode { get; set; } = string.Empty;

        public string CertName { get; set; } = string.Empty;

        public string CertificateType { get; set; } = string.Empty;

        public string ProviderName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<string> Specializations { get; set; } = [];

        public List<float>? Embedding { get; set; }
    }
}