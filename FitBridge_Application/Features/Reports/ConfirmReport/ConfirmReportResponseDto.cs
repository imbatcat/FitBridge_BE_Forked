namespace FitBridge_Application.Features.Reports.ConfirmReport
{
    public class ConfirmReportResponseDto
    {
        public bool IsMoreThanHalfCompleted { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int CompletedSessions { get; set; }
        public int TotalSessions { get; set; }
    }
}
