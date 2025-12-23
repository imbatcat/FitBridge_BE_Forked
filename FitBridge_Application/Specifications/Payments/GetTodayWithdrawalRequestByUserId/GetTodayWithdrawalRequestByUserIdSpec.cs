using System;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Specifications.Payments.GetTodayWithdrawalRequestByUserId;

public class GetTodayWithdrawalRequestByUserIdSpec : BaseSpecification<WithdrawalRequest>
{
    public GetTodayWithdrawalRequestByUserIdSpec(Guid accountId, DateTime date) : base(x =>
        x.IsEnabled
        && x.AccountId == accountId
        && x.CreatedAt.Date == date.Date
        && (x.Status == WithdrawalRequestStatus.Pending
        || x.Status == WithdrawalRequestStatus.AdminApproved
        || x.Status == WithdrawalRequestStatus.UserDisapproved
        || x.Status == WithdrawalRequestStatus.Resolved))
    {
    }
}
