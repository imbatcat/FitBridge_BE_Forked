using System;
using FitBridge_Domain.Exceptions;
using MediatR;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Utils;
using Microsoft.AspNetCore.Http;

namespace FitBridge_Application.Features.Accounts.GetGymOwnerMinimumSlot;

public class GetGymOwnerMinimumSlotQueryHandler(IApplicationUserService _applicationUserService, IUserUtil _userUtil, IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetGymOwnerMinimumSlotQuery, int>
{
    public async Task<int> Handle(GetGymOwnerMinimumSlotQuery request, CancellationToken cancellationToken)
    {
        var userId = _userUtil.GetAccountId(_httpContextAccessor.HttpContext);
        if (userId == null)
        {
            throw new NotFoundException("User not found");
        }
        var gymOwner = await _applicationUserService.GetByIdAsync(userId.Value);
        if (gymOwner == null)
        {
            throw new NotFoundException("Gym owner not found");
        }
        return gymOwner.MinimumSlot;
    }
}
