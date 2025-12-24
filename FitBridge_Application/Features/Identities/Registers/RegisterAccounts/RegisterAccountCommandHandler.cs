using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Identities;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Accounts;
using FitBridge_Domain.Entities.Identity;
using MediatR;
using Microsoft.Extensions.Configuration;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Application.Dtos.Emails;
using FitBridge_Domain.Exceptions;

namespace FitBridge_Application.Features.Identities.Registers.RegisterAccounts;

public class RegisterAccountCommandHandler(IApplicationUserService _applicationUserService, IConfiguration _configuration, IEmailService _emailService, IUnitOfWork _unitOfWork, IUploadService _uploadService) : IRequestHandler<RegisterAccountCommand, RegisterResponseDto>
{
    public async Task<RegisterResponseDto> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        if (request.Role != ProjectConstant.UserRoles.Admin
        && request.Role != ProjectConstant.UserRoles.FreelancePT
        && request.Role != ProjectConstant.UserRoles.GymOwner)
        {
            throw new BusinessException("Role not found, only Admin, FreelancePT and GymOwner are allowed to register");
        }
        if(request.OpenTime != null && request.CloseTime == null)
        {
            throw new BusinessException("Thiếu thời gian đóng cửa của gym");
        }
        if(request.OpenTime == null && request.CloseTime != null)
        {
            throw new BusinessException("Thiếu thời gian mở cửa của gym");
        }
        if (request.OpenTime != null && request.CloseTime != null)
        {
            if (request.OpenTime >= request.CloseTime)
            {
                throw new BusinessException("Thời gian mở cửa phải trước thời gian đóng cửa");
            }
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            Password = request.Password,
            GymName = request.GymName ?? "",
            TaxCode = request.TaxCode ?? null,
            Longitude = request.Longitude,
            Latitude = request.Latitude,
            EmailConfirmed = true,
            IsContractSigned = false,
            CitizenIdNumber = request.CitizenIdNumber ?? null,
            IdentityCardPlace = request.IdentityCardPlace ?? null,
            CitizenCardPermanentAddress = request.CitizenCardPermanentAddress ?? null,
            IdentityCardDate = request.IdentityCardDate ?? null,
            BusinessAddress = request.BusinessAddress ?? null,
            OpenTime = request.OpenTime ?? null,
            CloseTime = request.CloseTime ?? null,
            GymFoundationDate = request.GymFoundationDate ?? null,
            IsMale = request.IsMale ?? false,
            Dob = request.Dob ?? DateTime.UtcNow.AddYears(-17),
        };
        if (request.FrontCitizenIdFile != null)
        {
            user.FrontCitizenIdUrl = await _uploadService.UploadFileAsync(request.FrontCitizenIdFile);
        }
        if (request.BackCitizenIdFile != null)
        {
            user.BackCitizenIdUrl = await _uploadService.UploadFileAsync(request.BackCitizenIdFile);
        }
        await _applicationUserService.InsertUserAsync(user, request.Password);

        switch (request.Role)
        {
            case ProjectConstant.UserRoles.GymOwner:
                await InsertWallet(user);
                await _applicationUserService.AssignRoleAsync(user, ProjectConstant.UserRoles.GymOwner);
                await SendAccountInformationEmail(user, request.Password, request.IsTestAccount, ProjectConstant.UserRoles.GymOwner);
                await InsertUserDetail(user);
                break;
            case ProjectConstant.UserRoles.FreelancePT:
                await InsertWallet(user);
                await _applicationUserService.AssignRoleAsync(user, ProjectConstant.UserRoles.FreelancePT);
                await SendAccountInformationEmail(user, request.Password, request.IsTestAccount, ProjectConstant.UserRoles.FreelancePT);
                await InsertUserDetail(user);
                break;
            case ProjectConstant.UserRoles.Admin:
                await _applicationUserService.AssignRoleAsync(user, ProjectConstant.UserRoles.Admin);
                break;
        }
        await _unitOfWork.CommitAsync();

        return new RegisterResponseDto { UserId = user.Id };
    }

    public async Task InsertUserDetail(ApplicationUser user)
    {
        var userDetail = new UserDetail { Id = user.Id };
        _unitOfWork.Repository<UserDetail>().Insert(userDetail);
    }

    public async Task InsertWallet(ApplicationUser user)
    {
        var wallet = new Wallet { Id = user.Id, PendingBalance = 0, AvailableBalance = 0 };
        _unitOfWork.Repository<Wallet>().Insert(wallet);
    }

    public async Task SendAccountInformationEmail(ApplicationUser user, string password, bool isTestAccount, string role)
    {
        if (isTestAccount)
        {
            return;
        }
        var emailDate = new AccountInformationEmailData
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Password = password,
            Role = role,
            GymName = user.GymName,
            TaxCode = user.TaxCode,
            EmailType = ProjectConstant.EmailTypes.InformationEmail
        };
        await _emailService.ScheduleEmailAsync(emailDate);
    }
}
