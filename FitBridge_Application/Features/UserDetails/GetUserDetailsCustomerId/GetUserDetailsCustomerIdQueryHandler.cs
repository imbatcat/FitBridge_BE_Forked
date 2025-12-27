using System;
using FitBridge_Application.Dtos.Accounts.UserDetails;
using FitBridge_Application.Interfaces.Repositories;
using MediatR;
using AutoMapper;
using FitBridge_Domain.Entities.Accounts;

namespace FitBridge_Application.Features.UserDetails.GetUserDetailsCustomerId;

public class GetUserDetailsCustomerIdQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetUserDetailsCustomerIdQuery, UserDetailDto>   
{
    public async Task<UserDetailDto> Handle(GetUserDetailsCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var userDetail = await _unitOfWork.Repository<UserDetail>().GetByIdAsync(request.CustomerId, includes: new List<string> { "User" });
        return _mapper.Map<UserDetailDto>(userDetail);
    }
}
