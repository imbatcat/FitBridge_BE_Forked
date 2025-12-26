using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
namespace FitBridge_Application.Features.Accounts.UpdateGymPtMinimumSlot;

public class UpdateGymOwnerMinimumSlotCommandHandler(IUnitOfWork _unitOfWork, IUserUtil _userUtil, IHttpContextAccessor _httpContextAccessor, IApplicationUserService _applicationUserService) : IRequestHandler<UpdateGymOwnerMinimumSlotCommand, bool>
{
    public async Task<bool> Handle(UpdateGymOwnerMinimumSlotCommand request, CancellationToken cancellationToken)
    {
        var userId = _userUtil.GetAccountId(_httpContextAccessor.HttpContext);
        if (userId == null)
        {
            throw new NotFoundException("User not found");
        }
        var gymOwner = await _applicationUserService.GetByIdAsync(userId.Value);
        if (gymOwner == null)
        {
            throw new NotFoundException("Gym PT not found");
        }
        gymOwner.MinimumSlot = request.MinimumSlot;
        await _applicationUserService.UpdateAsync(gymOwner);
        await _unitOfWork.CommitAsync();
        return true;
    }
}
