using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Dtos.CustomerPurchaseds;

public class CustomerPurchasedTransactionHistoryDto
{
    public Guid CustomerPurchasedId { get; set; }
    public required string CustomerName { get; set; }
    public required string PackageName { get; set; }
    public int AvailableSessions { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public List<PurchaseTransactionDetailDto> Transactions { get; set; } = new List<PurchaseTransactionDetailDto>();
}

public class PurchaseTransactionDetailDto
{
    public Guid TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal MerchantProfit { get; set; }
    public required string Description { get; set; }
    public long OrderCode { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionStatus Status { get; set; }
    public required string PaymentMethod { get; set; }
    public Guid OrderId { get; set; }
}

