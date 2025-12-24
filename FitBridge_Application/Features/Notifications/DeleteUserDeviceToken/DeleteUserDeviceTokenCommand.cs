using MediatR;

namespace FitBridge_Application.Features.Notifications.DeleteUserDeviceToken
{
    public class DeleteUserDeviceTokenCommand : IRequest
    {
        public string DeviceToken { get; set; }
    }
}
