using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos.Notifications;
using FitBridge_Application.Dtos.Payments;
using FitBridge_Application.Dtos.Templates;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Services.Notifications;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Services;
using FitBridge_Application.Specifications.Payments.GetTodayWithdrawalRequestByUserId;
using FitBridge_Application.Specifications.Payments.GetWithdrawalRequestByUserIdSpec;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.MessageAndReview;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace FitBridge_Application.Features.Payments.CreateRequestPayment
{
    internal class CreateRequestPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IUserUtil userUtil,
        IHttpContextAccessor httpContextAccessor,
        IApplicationUserService applicationUserService,
        INotificationService notificationService,
        SystemConfigurationService systemConfigurationService) : IRequestHandler<CreateRequestPaymentCommand, RequestPaymentResponseDto>
    {
        public async Task<RequestPaymentResponseDto> Handle(CreateRequestPaymentCommand request, CancellationToken cancellationToken)
        {
            var accountId = userUtil.GetAccountId(httpContextAccessor.HttpContext)
                ?? throw new NotFoundException(nameof(ApplicationUser));
            var spec = new GetWithdrawalRequestByUserIdSpec(accountId);
            var withdrawalRequest = await unitOfWork.Repository<WithdrawalRequest>()
                .GetBySpecificationAsync(spec);

            if (withdrawalRequest != null)
            {
                throw new DuplicateException("User already registered a withdrawal request");
            }
            await CheckTodayWithdrawalLimit(accountId, request.Amount);

            var newWithdrawalRequest = InsertWithdrawalRequest(accountId, request);

            await ValidateWalletBalance(newWithdrawalRequest);

            await unitOfWork.CommitAsync();

            await SendNotification(request);

            return new RequestPaymentResponseDto { Id = newWithdrawalRequest.Id };
        }

        private async Task ValidateWalletBalance(WithdrawalRequest withdrawalRequest)
        {
            var wallet = await unitOfWork.Repository<Wallet>()
                .GetByIdAsync(withdrawalRequest.AccountId)
                ?? throw new NotFoundException($"Wallet not found for user with ID: {withdrawalRequest.AccountId}");

            if (wallet.AvailableBalance < withdrawalRequest.Amount)
            {
                throw new InsufficientWalletBalanceException(wallet.AvailableBalance, withdrawalRequest.Amount);
            }
        }

        private WithdrawalRequest InsertWithdrawalRequest(Guid accountId, CreateRequestPaymentCommand request)
        {
            var newWithdrawalRequest = new WithdrawalRequest
            {
                AccountId = accountId,
                Amount = request.Amount,
                BankName = request.BankName,
                Note = request.Note,
                AccountName = request.AccountName,
                AccountNumber = request.AccountNumber,
                Status = WithdrawalRequestStatus.Pending
            };

            unitOfWork.Repository<WithdrawalRequest>().Insert(newWithdrawalRequest);
            return newWithdrawalRequest;
        }

        private async Task SendNotification(CreateRequestPaymentCommand request)
        {
            var admins = await applicationUserService.GetUsersByRoleAsync(
                ProjectConstant.UserRoles.Admin);
            var requesterName = userUtil.GetUserFullName(httpContextAccessor.HttpContext);

            var model = new NewPaymentRequestModel
            {
                BodyAmmount = request.Amount,
                BodyRequesterName = requesterName ?? "Anonymous",
                TitleRequesterName = requesterName ?? "Anonymous"
            };
            var notificationMessage = new NotificationMessage(
                EnumContentType.NewPaymentRequest,
                admins.Select(admins => admins.Id).ToList(),
                model);

            await notificationService.NotifyUsers(notificationMessage);
        }

        private async Task CheckTodayWithdrawalLimit(Guid accountId, decimal withdrawalAmount)
        {
            var todayRequestSpec = new GetTodayWithdrawalRequestByUserIdSpec(accountId, DateTime.UtcNow);
            var todayRequest = await unitOfWork.Repository<WithdrawalRequest>()
                .GetAllWithSpecificationAsync(todayRequestSpec);
            var todayWithdrawAmount = todayRequest.Sum(x => x.Amount);
            var maximumWithdrawalAmountPerDay = (decimal)await systemConfigurationService.GetSystemConfigurationAutoConvertDataTypeAsync(ProjectConstant.SystemConfigurationKeys.MaximumWithdrawalAmountPerDay);
            if (todayWithdrawAmount + withdrawalAmount > maximumWithdrawalAmountPerDay)
            {
                throw new BusinessException($"Người dùng đã đạt tối đa số tiền được rút ra trong ngày, số tiền tối đa: {maximumWithdrawalAmountPerDay}đ. Số tiền đã rút hôm nay: {todayWithdrawAmount}đ, số tiền yêu cầu: {withdrawalAmount}đ");
            }
        }
    }
}