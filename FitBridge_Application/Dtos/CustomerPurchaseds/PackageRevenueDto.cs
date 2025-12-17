using System;

namespace FitBridge_Application.Dtos.CustomerPurchaseds
{
    public class PackageRevenueDto
    {
        public Guid PackageId { get; set; }
        public string PackageName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public int SalesCount { get; set; }
        public double RevenuePercentage { get; set; }
    }
}

