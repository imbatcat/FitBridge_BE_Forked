using System;

namespace FitBridge_Application.Dtos.Dashboard;

public class SummaryCardsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalProductProfit { get; set; }
    public decimal TotalGymCourseProfit { get; set; }
    public decimal TotalFreelanceProfit { get; set; }
    public decimal TotalSubscriptionProfit { get; set; }
}

