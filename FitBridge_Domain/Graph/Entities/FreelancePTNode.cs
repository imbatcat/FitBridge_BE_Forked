namespace FitBridge_Domain.Graph.Entities
{
    public class FreelancePTNode
    {
        public string DbId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsMale { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string BusinessAddress { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CourseDescription { get; set; } = string.Empty;
        public string CheapestCourse { get; set; } = string.Empty;
        public decimal CheapestPrice { get; set; }
        public string FreelancePtCourseId { get; set; } = string.Empty;
        public List<float>? Embedding { get; set; }
    }
}
