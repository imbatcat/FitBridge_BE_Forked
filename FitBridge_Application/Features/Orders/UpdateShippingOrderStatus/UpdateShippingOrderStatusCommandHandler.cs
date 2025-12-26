using System;
using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos.Orders;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Services;
using FitBridge_Domain.Entities.Ecommerce;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.Orders.UpdateShippingOrderStatus;

public class UpdateShippingOrderStatusCommandHandler : IRequestHandler<UpdateShippingOrderStatusCommand, OrderStatusResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScheduleJobServices _scheduleJobServices;
    private readonly SystemConfigurationService _systemConfigurationService;
    private readonly ILogger<UpdateShippingOrderStatusCommandHandler> _logger;

    public UpdateShippingOrderStatusCommandHandler(
        IUnitOfWork unitOfWork,
        IScheduleJobServices scheduleJobServices,
        SystemConfigurationService systemConfigurationService,
        ILogger<UpdateShippingOrderStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _scheduleJobServices = scheduleJobServices;
        _systemConfigurationService = systemConfigurationService;
        _logger = logger;
    }

    public async Task<OrderStatusResponseDto> Handle(UpdateShippingOrderStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Manual shipping status update for Order {request.OrderId} to {request.TargetStatus}");

        // Load order with all necessary includes
        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(
            request.OrderId,
            includes: new List<string>
            {
                "OrderItems",
                "OrderItems.ProductDetail",
                "Transactions",
                "Transactions.PaymentMethod",
                "OrderStatusHistories",
                "Coupon"
            });

        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }

        // Validate that this is a product order with shipping
        var productTransaction = order.Transactions.FirstOrDefault(t => t.TransactionType == TransactionType.ProductOrder);
        if (productTransaction == null)
        {
            throw new BusinessException("This is not a product order");
        }

        // Process the status update with Ahamove-like logic
        var oldStatus = order.Status;
        var (newStatus, statusDescription) = await ProcessShippingStatusUpdate(order, request);

        // Update order status
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        // Update existing status history description if status didn't change
        var statusHistoryToUpdate = order.OrderStatusHistories
            .Where(s => s.Status == oldStatus)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefault();

        if (statusHistoryToUpdate != null && oldStatus != newStatus)
        {
            statusHistoryToUpdate.Description = statusDescription;
        }

        // Insert new status history if status changed
        OrderStatusHistory newStatusHistory = null;
        if (oldStatus != newStatus)
        {
            newStatusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = newStatus,
                Description = statusDescription,
                PreviousStatus = oldStatus,
            };
            _unitOfWork.Repository<OrderStatusHistory>().Insert(newStatusHistory);
        }

        await _unitOfWork.CommitAsync();

        _logger.LogInformation($"Order {order.Id} status updated from {oldStatus} to {newStatus}. Description: {statusDescription}");

        // Return the latest status history or create a response
        var latestStatusHistory = order.OrderStatusHistories
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefault();

        return new OrderStatusResponseDto
        {
            Id = newStatusHistory?.Id ?? latestStatusHistory?.Id ?? Guid.NewGuid(),
            OrderId = order.Id,
            Status = newStatus,
            PreviousStatus = oldStatus,
            Description = statusDescription,
            CreatedAt = newStatusHistory?.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<(OrderStatus status, string description)> ProcessShippingStatusUpdate(
        Order order,
        UpdateShippingOrderStatusCommand request)
    {
        var targetStatus = request.TargetStatus;
        var oldStatus = order.Status;
        string description;

        switch (targetStatus)
        {
            case OrderStatus.Assigning:
                description = "Đang tìm tài xế";
                if (!string.IsNullOrEmpty(request.Comment))
                {
                    description += $" - {request.Comment}";
                }
                return (OrderStatus.Assigning, description);

            case OrderStatus.Accepted:
                var supplierName = string.IsNullOrEmpty(request.SupplierName) ? "tài xế" : request.SupplierName;
                description = $"Tài xế {supplierName} đã nhận đơn";
                if (!string.IsNullOrEmpty(request.Comment))
                {
                    description += $". {request.Comment}";
                }
                return (OrderStatus.Accepted, description);

            case OrderStatus.Shipping:
                // Handle actual shipping cost update
                if (oldStatus == OrderStatus.Accepted && request.ActualShippingCost.HasValue)
                {
                    order.ShippingFeeActualCost = request.ActualShippingCost.Value;
                    var shippingFeeDifference = order.ShippingFeeActualCost - order.ShippingFee;
                    var transactionToUpdate = order.Transactions.FirstOrDefault(t => t.TransactionType == TransactionType.ProductOrder && t.Status != TransactionStatus.Failed);
                    if (transactionToUpdate != null)
                    {
                        transactionToUpdate.ProfitAmount -= shippingFeeDifference;
                    }
                }

                description = "Đang giao hàng";
                if (!string.IsNullOrEmpty(request.Comment))
                {
                    description += $". {request.Comment}";
                }
                return (OrderStatus.Shipping, description);

            case OrderStatus.Arrived:
                // Handle arrival logic
                await HandleArrivedStatus(order);
                description = "Giao hàng thành công";
                if (!string.IsNullOrEmpty(request.Comment))
                {
                    description += $". {request.Comment}";
                }
                return (OrderStatus.Arrived, description);

            case OrderStatus.InReturn:
                description = "Đang hoàn trả hàng về người gửi";
                if (!string.IsNullOrEmpty(request.Comment))
                {
                    description = $"Đang hoàn trả hàng. Lý do: {request.Comment}";
                }
                return (OrderStatus.InReturn, description);

            case OrderStatus.Returned:
                // Handle return logic
                return await HandleReturnedStatus(order, request.Comment);

            default:
                throw new BusinessException($"Invalid target shipping status: {targetStatus}");
        }
    }

    private async Task HandleArrivedStatus(Order order)
    {
        var autoFinishArrivedOrderAfterTime = (int)await _systemConfigurationService
            .GetSystemConfigurationAutoConvertDataTypeAsync(
                ProjectConstant.SystemConfigurationKeys.AutoFinishArrivedOrderAfterTime);

        var transactionToUpdate = order.Transactions.FirstOrDefault(t => t.TransactionType == TransactionType.ProductOrder && t.Status != TransactionStatus.Failed);
        if (transactionToUpdate != null && transactionToUpdate.PaymentMethod.MethodType == MethodType.COD)
        {
            transactionToUpdate.Status = TransactionStatus.Success;
        }

        // Schedule auto-finish job
        await _scheduleJobServices.ScheduleAutoFinishArrivedOrderJob(
            order.Id,
            DateTime.UtcNow.AddDays(autoFinishArrivedOrderAfterTime));

        // Schedule feedback jobs
        var autoMarkAsFeedbackAfterDays = (int)await _systemConfigurationService
            .GetSystemConfigurationAutoConvertDataTypeAsync(
                ProjectConstant.SystemConfigurationKeys.AutoMarkAsFeedbackAfterDays);

        foreach (var orderItem in order.OrderItems)
        {
            await _scheduleJobServices.ScheduleAutoMarkAsFeedbackJob(
                orderItem.Id,
                DateTime.UtcNow.AddDays(autoMarkAsFeedbackAfterDays));
        }
    }

    private async Task<(OrderStatus status, string description)> HandleReturnedStatus(Order order, string comment)
    {
        var paymentMethod = await _unitOfWork.Repository<PaymentMethod>()
            .GetByIdAsync(order.Transactions.FirstOrDefault(t => t.TransactionType == TransactionType.ProductOrder)!.PaymentMethodId);

        if (paymentMethod == null)
        {
            throw new BusinessException("Payment method not found");
        }

        if (paymentMethod.MethodType == MethodType.COD)
        {
            // Create return status history entry
            var returnStatusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Returned,
                Description = $"Đã hoàn trả hàng. Lý do: {comment ?? "Không xác định"}",
                PreviousStatus = OrderStatus.InReturn,
            };

            // Return product quantities
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.ProductDetail != null)
                {
                    orderItem.ProductDetail.Quantity += orderItem.Quantity;
                    orderItem.ProductDetail.SoldQuantity -= orderItem.Quantity;
                }
            }

            _unitOfWork.Repository<OrderStatusHistory>().Insert(returnStatusHistory);
            return (OrderStatus.Cancelled, $"Đã hoàn trả hàng và hủy đơn hàng. Lý do: {comment ?? "Không xác định"}");
        } else {
            return (OrderStatus.Returned, $"Đã hoàn trả hàng. Lý do: {comment ?? "Không xác định"}");
        }
    }
}

