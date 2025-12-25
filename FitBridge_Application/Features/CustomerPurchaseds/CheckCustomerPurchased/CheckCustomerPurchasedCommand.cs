using System;
using FitBridge_Application.Dtos.CustomerPurchaseds;
using MediatR;

namespace FitBridge_Application.Features.CustomerPurchaseds.CheckCustomerPurchased;

public class CheckCustomerPurchasedCommand : IRequest<CheckCustomerPurchasedDto>
{
    public Guid? PtId { get; set; }

    public Guid? CustomerId { get; set; }
}