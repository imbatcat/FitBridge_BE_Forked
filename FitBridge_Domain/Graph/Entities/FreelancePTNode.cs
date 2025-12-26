using Neo4j.Driver.Mapping;

namespace FitBridge_Domain.Graph.Entities
{
    public class FreelancePTNode : BaseNode
    {
        [MappingSource("fpt.dbId")]
        public string DbId { get; set; } = string.Empty;

        [MappingSource("fpt.fullName")]
        public string FullName { get; set; } = string.Empty;

        [MappingSource("fpt.email")]
        public string Email { get; set; } = string.Empty;

        [MappingSource("fpt.phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MappingSource("fpt.isMale")]
        public bool IsMale { get; set; }

        [MappingSource("fpt.dob")]
        public DateTime DateOfBirth { get; set; }

        [MappingSource("fpt.businessAddress")]
        public string BusinessAddress { get; set; } = string.Empty;

        [MappingSource("fpt.lat")]
        public double Latitude { get; set; }

        [MappingSource("fpt.lon")]
        public double Longitude { get; set; }

        [MappingSource("fpt.courseDescription")]
        public string CourseDescription { get; set; } = string.Empty;

        [MappingSource("fpt.cheapestCourse")]
        public string CheapestCourse { get; set; } = string.Empty;

        [MappingSource("fpt.cheapestPrice")]
        public decimal CheapestPrice { get; set; }

        [MappingSource("fpt.freelancePtCourseId")]
        public string FreelancePtCourseId { get; set; } = string.Empty;

        [MappingSource("fpt.avgRating")]
        public double AverageRating { get; set; }

        [MappingSource("fpt.reviewCount")]
        public int ReviewCount { get; set; }
    }
}