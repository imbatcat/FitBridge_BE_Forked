using FitBridge_Domain.Entities.Gyms;

namespace FitBridge_Domain.Graph.Entities
{
    public class GymAssetNode : BaseNode
    {
        public string DbId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string AssetType { get; set; } = string.Empty;

        public List<float>? Embedding { get; set; }
    }
}