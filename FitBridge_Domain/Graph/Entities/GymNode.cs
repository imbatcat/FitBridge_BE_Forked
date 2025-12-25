namespace FitBridge_Domain.Graph.Entities
{
    public class GymNode
    {
        public string DbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BusinessAddress { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public string GymOwnerId { get; set; } = string.Empty;
        public string GymOwnerName { get; set; } = string.Empty;
        public string CheapestCourse { get; set; } = string.Empty;
        public decimal CheapestPrice { get; set; }
        public string CourseId { get; set; } = string.Empty;
        public List<float>? Embedding { get; set; }
    }
}
