using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Specifications.Notifications.GetByDeviceToken;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.MessageAndReview;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FitBridge_Application.Features.Notifications.DeleteUserDeviceToken
{
    internal class DeleteUserDeviceTokenCommandHandler(
        IHttpContextAccessor httpContextAccessor,
        IUserUtil userUtil,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteUserDeviceTokenCommand>
    {
        public async Task Handle(DeleteUserDeviceTokenCommand request, CancellationToken cancellationToken)
        {
            var accountId = userUtil.GetAccountId(httpContextAccessor.HttpContext)
                ?? throw new NotFoundException(nameof(ApplicationUser));

            var deviceToken = await unitOfWork.Repository<PushNotificationTokens>()
                .GetBySpecificationAsync(new GetByDeviceTokenSpec(request.DeviceToken, accountId));

            if (deviceToken != null)
            {
                unitOfWork.Repository<PushNotificationTokens>().Delete(deviceToken);
                await unitOfWork.CommitAsync();
            }
        }
    }
}