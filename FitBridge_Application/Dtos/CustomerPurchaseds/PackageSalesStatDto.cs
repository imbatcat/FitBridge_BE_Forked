using System;

namespace FitBridge_Application.Dtos.CustomerPurchaseds
{
    public class PackageSalesStatDto
    {
        public Guid PackageId { get; set; }
        public string PackageName { get; set; }
        public string PackageImageUrl { get; set; }
        public decimal PackagePrice { get; set; }
        public int TotalPackagesSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
    }
}


