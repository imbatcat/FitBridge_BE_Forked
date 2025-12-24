using FitBridge_Domain.Entities.MessageAndReview;
using System.Linq.Expressions;

namespace FitBridge_Application.Specifications.Notifications.GetByDeviceToken
{
    public class GetByDeviceTokenSpec : BaseSpecification<PushNotificationTokens>
    {
        public GetByDeviceTokenSpec(string deviceToken, Guid? userId = null) : base(x =>
            x.DeviceToken == deviceToken
            && (userId == null || x.UserId == userId))
        {
        }
    }
}