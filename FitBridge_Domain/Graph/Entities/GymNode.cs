using Neo4j.Driver.Mapping;

namespace FitBridge_Domain.Graph.Entities
{
    public class GymNode : BaseNode
    {
        [MappingSource("g.dbId")]
        public string DbId { get; set; } = string.Empty;

        [MappingSource("g.name")]
        public string Name { get; set; } = string.Empty;

        [MappingSource("g.email")]
        public string Email { get; set; } = string.Empty;

        [MappingSource("g.businessAddress")]
        public string BusinessAddress { get; set; } = string.Empty;

        [MappingSource("g.lat")]
        public double Latitude { get; set; }

        [MappingSource("g.lon")]
        public double Longitude { get; set; }

        [MappingSource("g.openTime")]
        public string OpenTime { get; set; } = string.Empty;

        [MappingSource("g.closeTime")]
        public string CloseTime { get; set; } = string.Empty;

        [MappingSource("g.avgRating")]
        public double AverageRating { get; set; }

        [MappingSource("g.gymOwnerId")]
        public string GymOwnerId { get; set; } = string.Empty;

        [MappingSource("g.gymOwnerName")]
        public string GymOwnerName { get; set; } = string.Empty;

        [MappingSource("g.cheapestCourse")]
        public string CheapestCourse { get; set; } = string.Empty;

        [MappingSource("g.cheapestPrice")]
        public decimal CheapestPrice { get; set; }

        [MappingSource("g.courseId")]
        public string CourseId { get; set; } = string.Empty;
    }
}