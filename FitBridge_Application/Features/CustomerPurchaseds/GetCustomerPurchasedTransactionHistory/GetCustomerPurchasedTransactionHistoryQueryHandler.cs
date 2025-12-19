using FitBridge_Application.Dtos.CustomerPurchaseds;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Services;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.CustomerPurchaseds.GetCustomerPurchasedTransactionHistory;

public class GetCustomerPurchasedTransactionHistoryQueryHandler(
    IUnitOfWork unitOfWork,
    ITransactionService _transactionService)
    : IRequestHandler<GetCustomerPurchasedTransactionHistoryQuery, CustomerPurchasedTransactionHistoryDto>
{
    public async Task<CustomerPurchasedTransactionHistoryDto> Handle(
        GetCustomerPurchasedTransactionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Get CustomerPurchased with all necessary includes
        var customerPurchased = await unitOfWork.Repository<CustomerPurchased>()
            .GetByIdAsync(request.CustomerPurchasedId, true, new List<string>
            {
                "Customer",
                "OrderItems.Order.Transactions.PaymentMethod",
                "OrderItems.Order.Coupon",
                "OrderItems.FreelancePTPackage",
                "OrderItems.GymCourse",
                "OrderThatExtend.Transactions.PaymentMethod",
                "OrderThatExtend.OrderItems",
                "OrderThatExtend.Coupon"
            });

        if (customerPurchased == null)
        {
            throw new NotFoundException(nameof(CustomerPurchased), request.CustomerPurchasedId);
        }

        var firstOrderItem = customerPurchased.OrderItems.FirstOrDefault();
        var packageName = firstOrderItem?.FreelancePTPackage?.Name 
            ?? firstOrderItem?.GymCourse?.Name 
            ?? "Unknown Package";

        var response = new CustomerPurchasedTransactionHistoryDto
        {
            CustomerPurchasedId = customerPurchased.Id,
            CustomerName = customerPurchased.Customer.FullName,
            PackageName = packageName,
            AvailableSessions = customerPurchased.AvailableSessions,
            ExpirationDate = customerPurchased.ExpirationDate,
            Transactions = new List<PurchaseTransactionDetailDto>()
        };

        foreach (var orderItem in customerPurchased.OrderItems)
        {
            if (orderItem.Order?.Transactions != null)
            {
                var merchantProfit = await _transactionService.CalculateMerchantProfit(
                    orderItem,
                    orderItem.Order.Coupon);
                var relatedTransaction = orderItem.Order.Transactions.Where(t => t.TransactionType != TransactionType.DistributeProfit && t.TransactionType != TransactionType.PendingDeduction && t.Status == TransactionStatus.Success).ToList();
                foreach (var transaction in relatedTransaction)
                {
                    response.Transactions.Add(new PurchaseTransactionDetailDto
                    {
                        TransactionId = transaction.Id,
                        TransactionDate = transaction.CreatedAt,
                        TotalAmount = orderItem.Order.TotalAmount,
                        MerchantProfit = merchantProfit,
                        Description = transaction.Description,
                        OrderCode = transaction.OrderCode,
                        TransactionType = transaction.TransactionType,
                        Status = transaction.Status,
                        PaymentMethod = transaction.PaymentMethod.MethodType.ToString(),
                        OrderId = orderItem.Order.Id
                    });
                }
            }
        }

        response.Transactions = response.Transactions
            .OrderByDescending(t => t.TransactionDate)
            .ToList();

        return response;
    }
}

