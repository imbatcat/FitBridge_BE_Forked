using System;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Dtos.Dashboard;

public class RecentTransactionDto
{
    public Guid TransactionId { get; set; }
    public long OrderCode { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? ProfitAmount { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Order Info
    public decimal OrderTotalAmount { get; set; }

    // Customer Info
    public string CustomerFullName { get; set; }
    public string CustomerContact { get; set; }
    public TransactionType TransactionType { get; set; }
    public Guid OrderId { get; set; }
}

