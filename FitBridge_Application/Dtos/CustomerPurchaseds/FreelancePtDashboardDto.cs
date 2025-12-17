using System;
using System.Collections.Generic;

namespace FitBridge_Application.Dtos.CustomerPurchaseds
{
    public class FreelancePtDashboardDto
    {
        public MonthlyStatisticsDto CurrentMonth { get; set; }
        public MonthlyStatisticsDto PreviousMonth { get; set; }
        public int CurrentActiveCustomers { get; set; }
        public List<PackageSalesStatDto> MostPopularPackages { get; set; } = new List<PackageSalesStatDto>();
        public List<PackageRevenueDto> PackageRevenueBreakdown { get; set; } = new List<PackageRevenueDto>();
    }
}

