namespace FitBridge_Domain.Graph.Entities.Relationships
{
    public class OwnsRelationship
    {
        public string GymOwnerId { get; set; } = string.Empty;

        public string GymId { get; set; } = string.Empty;

        public DateTime OwnershipStartDate { get; set; }
    }
}