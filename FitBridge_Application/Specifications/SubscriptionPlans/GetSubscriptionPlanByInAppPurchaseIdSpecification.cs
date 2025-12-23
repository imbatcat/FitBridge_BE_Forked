using System;
using FitBridge_Domain.Entities.ServicePackages;

namespace FitBridge_Application.Specifications.SubscriptionPlans;

public class GetSubscriptionPlanByInAppPurchaseIdSpecification : BaseSpecification<SubscriptionPlansInformation>
{
    public GetSubscriptionPlanByInAppPurchaseIdSpecification(string inAppPurchaseId) 
        : base(x => x.InAppPurchaseId == inAppPurchaseId)
    {
        AddInclude(x => x.FeatureKey);
    }
}



