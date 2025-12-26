using FitBridge_API.Helpers.RequestHelpers;
using FitBridge_Application.Dtos.Orders;
using FitBridge_Application.Dtos.Reviews;
using FitBridge_Application.Dtos.Shippings;
using FitBridge_Application.Features.Courses.GetCourseCompletion;
using FitBridge_Application.Features.Orders.CancelShippingOrder;
using FitBridge_Application.Features.Orders.CreateOrders;
using FitBridge_Application.Features.Orders.CreateShippingOrder;
using FitBridge_Application.Features.Orders.GetAllProductOrder;
using FitBridge_Application.Features.Orders.GetCourseOrders;
using FitBridge_Application.Features.Orders.GetCustomerOrderHistory;
using FitBridge_Application.Features.Orders.GetOrderByCustomerPurchasedId;
using FitBridge_Application.Features.Orders.GetShippingPrice;
using FitBridge_Application.Features.Orders.ProcessAhamoveWebhook;
using FitBridge_Application.Features.Orders.UpdateOrderStatus;
using FitBridge_Application.Features.Reviews.GetCustomerReviews;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.Orders.GetAllProductOrders;
using FitBridge_Application.Specifications.Orders.GetCourseOrders;
using FitBridge_Application.Specifications.Orders.GetCustomerOrderHistory;
using FitBridge_Application.Specifications.Reviews.GetAllReviewForCustomer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitBridge_API.Controllers;

public class OrdersController(IMediator _mediator) : _BaseApiController
{
    // [HttpPost]
    // public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand orderDto)
    // {
    //     var order = await _mediator.Send(orderDto);
    //     return Ok(new BaseResponse<string>(StatusCodes.Status200OK.ToString(), "Order created successfully", order));
    // }

    [HttpGet("customer-purchased/{customerPurchasedId}")]
    public async Task<IActionResult> GetOrderByCustomerPurchasedId([FromRoute] Guid customerPurchasedId)
    {
        var order = await _mediator.Send(new GetOrderByCustomerPurchasedIdQuery { CustomerPurchasedId = customerPurchasedId });
        return Ok(new BaseResponse<OrderResponseDto>(StatusCodes.Status200OK.ToString(), "Order retrieved successfully", order));
    }

    /// <summary>
    /// Create a shipping order for a product order
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("shipping")]
    public async Task<IActionResult> CreateShippingOrder([FromBody] CreateShippingOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new BaseResponse<CreateShippingOrderResponseDto>(StatusCodes.Status200OK.ToString(), "Shipping order created successfully", result));
    }

    /// <summary>
    /// API for ahamove to callback when there is a change in the shipping order status
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("shipping/webhook")]
    public async Task<IActionResult> ShippingCallbackWebhook()
    {
        var reader = new StreamReader(Request.Body);
        var webhookPayload = await reader.ReadToEndAsync();

        // Use StreamReader to read the raw body since the data comes as a nested JSON string
        // using var reader = new StreamReader(Request.Body);
        // var webhookPayload = await reader.ReadToEndAsync();

        var command = new ProcessAhamoveWebhookCommand
        {
            WebhookPayload = webhookPayload
        };

        await _mediator.Send(command);

        return Ok(new BaseResponse<string>(StatusCodes.Status200OK.ToString(), "Shipping webhook processed successfully", webhookPayload));
    }

    /// <summary>
    /// Update the status of an order
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPut("status/{orderId}")]
    public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid orderId, [FromBody] UpdateOrderStatusCommand command)
    {
        command.OrderId = orderId;
        var result = await _mediator.Send(command);
        return Ok(new BaseResponse<OrderStatusResponseDto>(StatusCodes.Status200OK.ToString(), "Order status updated successfully", result));
    }

    /// <summary>
    /// Cancel a shipping order
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPut("shipping/cancel/{orderId}")]
    public async Task<IActionResult> CancelShippingOrder([FromRoute] Guid orderId, [FromBody] CancelShippingOrderCommand command)
    {
        command.OrderId = orderId;
        var result = await _mediator.Send(command);
        return Ok(new BaseResponse<bool>(StatusCodes.Status200OK.ToString(), "Shipping order canceled successfully", result));
    }

    /// <summary>
    /// Estimate the shipping price for a product order
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("shipping/price-estimate")]
    public async Task<IActionResult> GetShippingPrice([FromBody] GetShippingPriceCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new BaseResponse<ShippingEstimateDto>(StatusCodes.Status200OK.ToString(), "Shipping price retrieved successfully", result));
    }

    /// <summary>
    /// Get all product orders
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    [HttpGet("product")]
    public async Task<IActionResult> GetAllProductOrders([FromQuery] GetAllProductOrdersParams parameters)
    {
        var result = await _mediator.Send(new GetAllProductOrdersQuery { Params = parameters });
        var pagination = ResultWithPagination(result.ProductOrders.Items, result.ProductOrders.Total, parameters.Page, parameters.Size);
        var response = new
        {
            result.SummaryProductOrder,
            ProductOrders = pagination
        };
        return Ok(new BaseResponse<object>(StatusCodes.Status200OK.ToString(), "Orders retrieved successfully", response));
    }

    /// <summary>
    /// Get all course orders, can be freelance pt course or gym course of a customer
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    [HttpGet("course")]
    public async Task<IActionResult> GetAllCourseOrders([FromQuery] GetCourseOrderParams parameters)
    {
        var result = await _mediator.Send(new GetCourseOrderQuery(parameters));
        var pagination = ResultWithPagination(result.Items, result.Total, parameters.Page, parameters.Size);
        return Ok(new BaseResponse<Pagination<CourseOrderResponseDto>>(StatusCodes.Status200OK.ToString(), "Course orders retrieved successfully", pagination));
    }

    /// <summary>
    /// Get customer order transaction history for training packages
    /// </summary>
    /// <param name="parameters">Query parameters for filtering and pagination</param>
    /// <returns>Returns paginated list of orders with their items and related transactions</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/v1/orders/customer/history?page=1&amp;size=10&amp;sortBy=CreatedAt&amp;sortOrder=desc
    ///     GET /api/v1/orders/customer/history?orderId=3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     GET /api/v1/orders/customer/history?orderStatus=Finished
    ///
    /// This endpoint retrieves all orders made by the authenticated customer that contain
    /// training package purchases (Gym Courses or Freelance PT Packages), including:
    /// - Initial package purchases (GymCourse, FreelancePTPackage)
    /// - Package extensions (ExtendCourse, ExtendFreelancePTPackage)
    ///
    /// **Query Parameters:**
    /// - `orderId` (optional): Filter by specific order ID
    /// - `orderStatus` (optional): Filter by order status (Created, Pending, Finished, Cancelled, etc.)
    /// - `sortBy` (optional): Sort by field (CreatedAt, TotalAmount, OrderStatus). Default: CreatedAt
    /// - `sortOrder` (optional): Sort order (asc, desc). Default: desc
    /// - `page` (optional): Page number. Default: 1
    /// - `size` (optional): Page size. Default: 10
    ///
    /// Each order includes:
    /// - Order summary information (status, total amount, creation date)
    /// - List of purchased items (package name, image, price, quantity)
    /// - List of related transactions (payment details, status, transaction date)
    ///
    /// Transactions are filtered to show only training package-related transactions,
    /// excluding internal system transactions like profit distributions.
    /// </remarks>
    /// <response code="200">Order history retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">User not found</response>
    [HttpGet("customer/history")]
    [ProducesResponseType(typeof(BaseResponse<Pagination<CustomerOrderHistoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerOrderHistory([FromQuery] GetCustomerOrderHistoryParams parameters)
    {
        var query = new GetCustomerOrderHistoryQuery(parameters);
        var result = await _mediator.Send(query);
        var pagination = ResultWithPagination(result.Items, result.Total, parameters.Page, parameters.Size);
        return Ok(new BaseResponse<Pagination<CustomerOrderHistoryDto>>(
            StatusCodes.Status200OK.ToString(),
            "Customer order history retrieved successfully",
            pagination));
    }

    /// <summary>
    /// Check the completion status of a course (Gym Course or Freelance PT Package)
    /// </summary>
    /// <param name="orderItemId">The ID of the order item representing the purchased course</param>
    /// <returns>Detailed completion information including progress, sessions count, and completion percentage</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/v1/orders/course/completion-check/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// This endpoint provides comprehensive information about a customer's progress in a purchased course:
    /// - Total number of sessions in the course
    /// - Number of completed sessions
    /// - Number of cancelled sessions
    /// - Number of upcoming/booked sessions
    /// - Available sessions remaining
    /// - Completion percentage
    /// - Whether the course is fully completed
    /// - Course expiration date
    ///
    /// **Response includes:**
    /// - `orderItemId`: The order item ID
    /// - `customerPurchasedId`: The customer purchased record ID (if applicable)
    /// - `courseName`: Name of the gym course or freelance PT package
    /// - `isGymCourse`: True if gym course, false if freelance PT package
    /// - `totalSessions`: Total sessions in the course
    /// - `completedSessions`: Number of finished sessions
    /// - `cancelledSessions`: Number of cancelled sessions
    /// - `upcomingSessions`: Number of booked or pending sessions
    /// - `availableSessions`: Remaining sessions available to book
    /// - `completionPercentage`: Percentage of course completion (0-100)
    /// - `isCompleted`: Whether all sessions are completed
    /// - `expirationDate`: Course expiration date
    ///
    /// **Use Cases:**
    /// - Display course progress to customers
    /// - Determine if refund is applicable based on completion status
    /// - Track training progress for reporting
    /// </remarks>
    /// <response code="200">Course completion information retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Order item not found or invalid course type</response>
    [HttpGet("course/completion-check/{orderItemId}")]
    [ProducesResponseType(typeof(BaseResponse<CourseCompletionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseCompletion([FromRoute] Guid orderItemId)
    {
        var query = new GetCourseCompletionQuery { OrderItemId = orderItemId };
        var result = await _mediator.Send(query);
        return Ok(new BaseResponse<CourseCompletionResult>(
            StatusCodes.Status200OK.ToString(),
            "Course completion information retrieved successfully",
            result));
    }
}