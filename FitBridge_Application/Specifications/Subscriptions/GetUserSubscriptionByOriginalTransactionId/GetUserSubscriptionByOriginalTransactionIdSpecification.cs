using System;
using System.Linq.Expressions;
using FitBridge_Domain.Entities.ServicePackages;

namespace FitBridge_Application.Specifications.Subscriptions.GetUserSubscriptionByOriginalTransactionId;

public class GetUserSubscriptionByOriginalTransactionIdSpecification : BaseSpecification<UserSubscription>
{
    public GetUserSubscriptionByOriginalTransactionIdSpecification(string originalTransactionId) : base(x => x.OriginalTransactionId == originalTransactionId)
    {
        AddInclude(x => x.User);
        AddInclude(x => x.SubscriptionPlansInformation);
        AddInclude("SubscriptionPlansInformation.FeatureKey");
        AddOrderBy(x => x.CreatedAt);
    }
}



