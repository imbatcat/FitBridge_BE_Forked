namespace FitBridge_Application.Interfaces.Services
{
    public interface ICourseCompletionService
    {
        Task<CourseCompletionResult> GetCourseCompletionAsync(Guid orderItemId);
        Task<bool> IsCourseCompletedAsync(Guid orderItemId);
    }

    public class CourseCompletionResult
    {
        public Guid OrderItemId { get; set; }
        public Guid? CustomerPurchasedId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public bool IsGymCourse { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int CancelledSessions { get; set; }
        public int UpcomingSessions { get; set; }
        public int AvailableSessions { get; set; }
        public decimal CompletionPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateOnly? ExpirationDate { get; set; }
    }
}
