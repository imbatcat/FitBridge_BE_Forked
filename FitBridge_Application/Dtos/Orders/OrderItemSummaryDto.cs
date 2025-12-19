using System;

namespace FitBridge_Application.Dtos.Orders
{
    public class OrderItemSummaryDto
    {
        public Guid OrderItemId { get; set; }
        public string ItemName { get; set; }
        public Guid? FreelancePTPackageId { get; set; }
        public Guid? GymCourseId { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}

