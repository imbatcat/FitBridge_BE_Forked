using System;
using FitBridge_Application.Dtos.Orders;
using FitBridge_Domain.Enums.Orders;
using MediatR;
using System.Text.Json.Serialization;

namespace FitBridge_Application.Features.Orders.UpdateShippingOrderStatus;

public class UpdateShippingOrderStatusCommand : IRequest<OrderStatusResponseDto>
{
    [JsonIgnore]
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Target shipping status to update to (Assigning, Accepted, Shipping, Arrived, InReturn, Returned)
    /// </summary>
    public OrderStatus TargetStatus { get; set; }
    
    /// <summary>
    /// Optional description or comment for the status update
    /// </summary>
    public string? Comment { get; set; }
    
    /// <summary>
    /// Optional supplier/driver name (for Accepted status)
    /// </summary>
    public string? SupplierName { get; set; }
    
    /// <summary>
    /// Optional actual shipping cost (for Shipping status when transitioning from Accepted)
    /// </summary>
    public decimal? ActualShippingCost { get; set; }
}

