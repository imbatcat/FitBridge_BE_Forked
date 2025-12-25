namespace FitBridge_Domain.Graph.Entities.Relationships
{
    public class TargetsRelationship
    {
        public string SourceId { get; set; } = string.Empty;

        public string MuscleId { get; set; } = string.Empty;

        public int TargetIntensity { get; set; }
    }
}