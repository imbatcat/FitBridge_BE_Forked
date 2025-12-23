using System;

namespace FitBridge_Application.Dtos.Dashboard;

public class ChartPointDto
{
    public DateTime Date { get; set; }
    
    // Product
    public decimal ProductRevenue { get; set; }
    public decimal ProductProfit { get; set; }
    
    // Gym
    public decimal GymRevenue { get; set; }
    public decimal GymProfit { get; set; }
    
    // Freelance
    public decimal FreelanceRevenue { get; set; }
    public decimal FreelanceProfit { get; set; }
    
    // Subscription
    public decimal SubRevenue { get; set; }
    public decimal SubProfit { get; set; }
}

