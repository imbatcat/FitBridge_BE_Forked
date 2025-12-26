using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Identities;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Accounts;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Graph.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Application.Dtos.Emails;
using FitBridge_Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.Identities.Registers.RegisterAccounts;

public class RegisterAccountCommandHandler(
    IApplicationUserService _applicationUserService, 
    IConfiguration _configuration, 
    IEmailService _emailService, 
    IUnitOfWork _unitOfWork, 
    IUploadService _uploadService,
    IGraphService _graphService,
    ILogger<RegisterAccountCommandHandler> _logger) : IRequestHandler<RegisterAccountCommand, RegisterResponseDto>
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
            Dob = DateTime.SpecifyKind(request.Dob ?? DateTime.UtcNow.AddYears(-17), DateTimeKind.Utc),
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

        // Create graph nodes after commit (non-blocking)
        if (request.Role == ProjectConstant.UserRoles.GymOwner)
        {
            await CreateGymNode(user);
        }
        else if (request.Role == ProjectConstant.UserRoles.FreelancePT)
        {
            await CreateFreelancePTNode(user);
        }

        return new RegisterResponseDto { UserId = user.Id };
    }

    private async Task CreateFreelancePTNode(ApplicationUser user)
    {
        try
        {
            var freelancePTNode = new FreelancePTNode
            {
                DbId = user.Id.ToString(),
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsMale = user.IsMale,
                DateOfBirth = user.Dob,
                BusinessAddress = user.BusinessAddress ?? string.Empty,
                Latitude = user.Latitude ?? 0,
                Longitude = user.Longitude ?? 0,
                CourseDescription = string.Empty,
                CheapestCourse = string.Empty,
                CheapestPrice = 0,
                FreelancePtCourseId = string.Empty
            };
            
            await _graphService.CreateNode(freelancePTNode);
            _logger.LogInformation("Created FreelancePT node for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create FreelancePT node for user {UserId}: {Message}", user.Id, ex.Message);
        }
    }

    private async Task CreateGymNode(ApplicationUser user)
    {
        try
        {
            var gymNode = new GymNode
            {
                DbId = user.Id.ToString(),
                Name = user.GymName ?? string.Empty,
                Email = user.Email,
                BusinessAddress = user.BusinessAddress ?? string.Empty,
                Latitude = user.Latitude ?? 0,
                Longitude = user.Longitude ?? 0,
                OpenTime = user.OpenTime?.ToString() ?? string.Empty,
                CloseTime = user.CloseTime?.ToString() ?? string.Empty,
                AverageRating = 0,
                GymOwnerId = user.Id.ToString(),
                GymOwnerName = user.FullName,
                CheapestCourse = string.Empty,
                CheapestPrice = 0,
                CourseId = string.Empty
            };
            
            await _graphService.CreateNode(gymNode);
            _logger.LogInformation("Created Gym node for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create Gym node for user {UserId}: {Message}", user.Id, ex.Message);
        }
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
