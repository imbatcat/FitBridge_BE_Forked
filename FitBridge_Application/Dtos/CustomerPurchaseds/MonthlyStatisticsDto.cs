using System;

namespace FitBridge_Application.Dtos.CustomerPurchaseds
{
    public class MonthlyStatisticsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalPackagesSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int NewCustomers { get; set; }
    }
}

