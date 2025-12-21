using System;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Specifications.PaymentMethods;

public class GetPaymentMethodByTypeSpecification : BaseSpecification<PaymentMethod>
{
    public GetPaymentMethodByTypeSpecification(MethodType methodType) 
        : base(x => x.MethodType == methodType)
    {
    }
}



