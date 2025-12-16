using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Dtos.Transactions
{
    public class GetTransactionsDto
    {
        public Guid Id { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal Amount { get; set; }
        public long OrderCode { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string Description { get; set; }
        public required string PaymentMethod { get; set; }
        public decimal? ProfitAmount { get; set; }

        // Purchased item information
        public string? PurchasedItemName { get; set; }
        public string? PurchasedItemType { get; set; }

        // Customer information
        public string? CustomerName { get; set; }
        public Guid? CustomerId { get; set; }

        // Related entity IDs
        public Guid? OrderId { get; set; }
        public Guid? CustomerPurchasedId { get; set; }
        public int Quantity { get; set; }
    }
}