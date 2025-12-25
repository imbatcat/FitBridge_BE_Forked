using System;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Specifications.CustomerPurchaseds.GetCustomerPurchasedByFreelancePtId;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Exceptions;
using FitBridge_Application.Interfaces.Utils;
using Microsoft.AspNetCore.Http;
using MediatR;
using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos.CustomerPurchaseds;

namespace FitBridge_Application.Features.CustomerPurchaseds.CheckCustomerPurchased;

public class CheckCustomerPurchasedCommandHandler(IUnitOfWork _unitOfWork, IUserUtil _userUtil, IHttpContextAccessor _httpContextAccessor) : IRequestHandler<CheckCustomerPurchasedCommand, CheckCustomerPurchasedDto>
{
    public async Task<CheckCustomerPurchasedDto> Handle(CheckCustomerPurchasedCommand request, CancellationToken cancellationToken)
    {
        var userId = _userUtil.GetAccountId(_httpContextAccessor.HttpContext);
        var userRole = _userUtil.GetUserRole(_httpContextAccessor.HttpContext);
        if (userId == null)
        {
            throw new NotFoundException("User not found");
        }

        if (request.CustomerId == null && request.PtId == null)
        {
            throw new DataValidationFailedException("Must provide either customer id or freelance pt id");
        }

        Guid ptId;
        Guid customerId;
        switch (userRole)
        {
            case ProjectConstant.UserRoles.Customer:
                if (request.PtId == null)
                {
                    throw new DataValidationFailedException("Must provide freelance pt id");
                }
                customerId = userId.Value;
                ptId = request.PtId.Value;
                break;

            case ProjectConstant.UserRoles.FreelancePT:
                if (request.CustomerId == null)
                {
                    throw new DataValidationFailedException("Must provide customer id");
                }
                ptId = userId.Value;
                customerId = request.CustomerId.Value;
                break;

            default:
                throw new BusinessException("Unauthorized role");
        }

        var customerPurchased = await _unitOfWork.Repository<CustomerPurchased>()
            .GetBySpecificationAsync(new GetCustomerPurchasedByFreelancePtIdSpec(ptId, customerId));

        if (customerPurchased == null)
        {
            throw new NotFoundException("Customer purchased not found");
        }

        var dto = new CheckCustomerPurchasedDto
        {
            Id = customerPurchased.Id,
        };
        var ptFreelancePackage = customerPurchased.OrderItems.FirstOrDefault(x => x.FreelancePTPackageId != null);

        if (ptFreelancePackage != null && ptFreelancePackage.FreelancePTPackage != null)
        {
            dto.SessionDurationInMinutes = ptFreelancePackage.FreelancePTPackage.SessionDurationInMinutes;
        }

        return dto;
    }
}