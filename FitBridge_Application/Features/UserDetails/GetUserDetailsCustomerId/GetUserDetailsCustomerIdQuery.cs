using System;
using FitBridge_Application.Dtos.Accounts.UserDetails;
using MediatR;

namespace FitBridge_Application.Features.UserDetails.GetUserDetailsCustomerId;

public class GetUserDetailsCustomerIdQuery : IRequest<UserDetailDto>
{
    public Guid CustomerId { get; set; }
}
