using System;

namespace FitBridge_Application.Dtos.Orders
{
    public class CustomerTransactionDetailDto
    {
        public Guid TransactionId { get; set; }
        public long OrderCode { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string PaymentMethodName { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}

