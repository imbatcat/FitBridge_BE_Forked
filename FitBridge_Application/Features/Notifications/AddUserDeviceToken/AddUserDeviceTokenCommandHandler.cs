using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Specifications.Notifications;
using FitBridge_Application.Specifications.Notifications.GetByDeviceToken;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.MessageAndReview;
using FitBridge_Domain.Enums.Notifications;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.Notifications.AddUserDeviceToken
{
    internal class AddUserDeviceTokenCommandHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger logger,
        IUserUtil userUtil,
        IUnitOfWork unitOfWork) : IRequestHandler<AddUserDeviceTokenCommand>
    {
        public async Task Handle(AddUserDeviceTokenCommand request, CancellationToken cancellationToken)
        {
            var accountId = userUtil.GetAccountId(httpContextAccessor.HttpContext)
                ?? throw new NotFoundException(nameof(ApplicationUser));

            var deviceToken = await unitOfWork.Repository<PushNotificationTokens>()
                .GetBySpecificationAsync(new GetByDeviceTokenSpec(request.DeviceToken, accountId));
            if (deviceToken != null)
            {
                return;
            }

            var newToken = new PushNotificationTokens
            {
                Id = Guid.NewGuid(),
                DeviceToken = request.DeviceToken,
                UserId = accountId,
                Platform = request.Platform,
                IsEnabled = true
            };

            unitOfWork.Repository<PushNotificationTokens>().Insert(newToken);

            try
            {
                await unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                logger.LogInformation("Duplicate device token detected: {DeviceToken} for User: {UserId}", request.DeviceToken, accountId);
            }
        }
    }
}