using System;
using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos.Payments;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Services;
using FitBridge_Application.Specifications.Payments.GetTodayWithdrawalRequestByUserId;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FitBridge_Application.Features.Payments.CheckWithdrawalMaximumAmount;

public class CheckWithdrawalMaximumAmountCommandHandler(
    IUnitOfWork unitOfWork,
    IUserUtil userUtil,
    IHttpContextAccessor httpContextAccessor,
    SystemConfigurationService systemConfigurationService) : IRequestHandler<CheckWithdrawalMaximumAmountCommand, CheckWithdrawalMaximumAmountDto>
{
    public async Task<CheckWithdrawalMaximumAmountDto> Handle(CheckWithdrawalMaximumAmountCommand request, CancellationToken cancellationToken)
    {
        var accountId = userUtil.GetAccountId(httpContextAccessor.HttpContext)
            ?? throw new NotFoundException(nameof(ApplicationUser));
        var todayRequestSpec = new GetTodayWithdrawalRequestByUserIdSpec(accountId, DateTime.UtcNow);
        var todayRequest = await unitOfWork.Repository<WithdrawalRequest>()
            .GetAllWithSpecificationAsync(todayRequestSpec);
        var todayWithdrawAmount = todayRequest.Sum(x => x.Amount);
        var maximumWithdrawalAmountPerDay = (decimal)await systemConfigurationService.GetSystemConfigurationAutoConvertDataTypeAsync(ProjectConstant.SystemConfigurationKeys.MaximumWithdrawalAmountPerDay);
        return new CheckWithdrawalMaximumAmountDto
        {
            MaximumWithdrawalAmountPerDay = maximumWithdrawalAmountPerDay,
            TodayWithdrawalAmount = todayWithdrawAmount,
            IsMaximumWithdrawalAmountReached = todayWithdrawAmount >= maximumWithdrawalAmountPerDay
        };
    }
}
