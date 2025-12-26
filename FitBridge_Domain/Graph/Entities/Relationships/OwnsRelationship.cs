namespace FitBridge_Domain.Graph.Entities.Relationships
{
    public class OwnsRelationship : BaseRelationship
    {
        public string GymOwnerId { get; set; } = string.Empty;

        public string GymAssetId { get; set; } = string.Empty;
    }
}