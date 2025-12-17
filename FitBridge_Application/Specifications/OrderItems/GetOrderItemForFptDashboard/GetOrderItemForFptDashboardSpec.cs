using System;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Specifications.OrderItems.GetOrderItemForFptDashboard;

public class GetOrderItemForFptDashboardSpec : BaseSpecification<OrderItem>
{
    public GetOrderItemForFptDashboardSpec(Guid ptId) : base(x => x.IsEnabled
        && x.FreelancePTPackageId != null
        && x.FreelancePTPackage.PtId == ptId
        && x.CustomerPurchased != null
        && x.Order.Status == OrderStatus.Finished)
    {
        AddInclude(x => x.FreelancePTPackage);
        AddInclude(x => x.CustomerPurchased);
        AddInclude(x => x.Order);
        AddInclude(x => x.Order.Coupon);
    }
}
