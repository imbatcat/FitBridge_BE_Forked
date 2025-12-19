using System;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Specifications.Orders.GetCustomerOrderHistory
{
    public class GetCustomerOrderHistoryParams : BaseParams
    {
        public Guid? OrderId { get; set; }
        public OrderStatus? OrderStatus { get; set; }
    }
}

