using System;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
namespace FitBridge_Application.Features.Accounts.UpdateGymPtMinimumSlot;

public class UpdateGymPtMinimumSlotCommandHandler(IUnitOfWork _unitOfWork, IUserUtil _userUtil, IHttpContextAccessor _httpContextAccessor, IApplicationUserService _applicationUserService) : IRequestHandler<UpdateGymPtMinimumSlotCommand, bool>
{
    public async Task<bool> Handle(UpdateGymPtMinimumSlotCommand request, CancellationToken cancellationToken)
    {
        var userId = _userUtil.GetAccountId(_httpContextAccessor.HttpContext);
        if (userId == null)
        {
            throw new NotFoundException("User not found");
        }
        var gymPt = await _applicationUserService.GetByIdAsync(userId.Value);
        if (gymPt == null)
        {
            throw new NotFoundException("Gym PT not found");
        }
        gymPt.MinimumSlot = request.MinimumSlot;
        await _applicationUserService.UpdateAsync(gymPt);
        await _unitOfWork.CommitAsync();
        return true;
    }
}
