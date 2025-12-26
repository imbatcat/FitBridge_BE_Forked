namespace FitBridge_Domain.Graph.Entities.Relationships
{
    public class TargetsRelationship : BaseRelationship
    {
        public string GymAssetId { get; set; } = string.Empty;

        public List<string> MuscleNames { get; set; } = [];
    }
}