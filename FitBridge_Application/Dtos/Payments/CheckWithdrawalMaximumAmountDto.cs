using System;

namespace FitBridge_Application.Dtos.Payments;

public class CheckWithdrawalMaximumAmountDto
{
    public decimal MaximumWithdrawalAmountPerDay { get; set; }
    public decimal TodayWithdrawalAmount { get; set; }
    public bool IsMaximumWithdrawalAmountReached { get; set; }
}
