using System;
using FitBridge_Application.Dtos.Payments;
using MediatR;

namespace FitBridge_Application.Features.Payments.CheckWithdrawalMaximumAmount;

public class CheckWithdrawalMaximumAmountCommand : IRequest<CheckWithdrawalMaximumAmountDto>
{
}
