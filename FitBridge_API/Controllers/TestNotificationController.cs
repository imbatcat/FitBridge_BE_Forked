using FitBridge_Application.Dtos.Notifications;
using FitBridge_Application.Dtos.Templates;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services.Notifications;
using FitBridge_Domain.Enums.MessageAndReview;
using FitBridge_Infrastructure.Services.Notifications.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FitBridge_API.Controllers
{
    public class TestNotificationController(
        INotificationService notificationService,
        NotificationConnectionManager notificationConnectionManager) : _BaseApiController
    {
        public record TestNotiDto(string Body, string Title, Guid userId);

        [HttpPost]
        public async Task<IActionResult> TestNotification([FromBody] TestNotiDto message)
        {
            await notificationConnectionManager.AddConnectionAsync(message.userId.ToString(), "connectionId1");
            var uid = message.userId;
            await notificationService.NotifyUsers(new NotificationMessage(
                EnumContentType.RemindBookingSession,
                new List<Guid> { Guid.Parse("01998f92-369f-79a2-9764-051f6342a824"), Guid.Parse("019b07e1-0d80-77cf-9c88-266a7ff0c633") },
                new RemindBookingSessionModel("Booking Name", "10:00", "2025-01-01")
            ));
            return Ok(new { Message = "Notification sent successfully." });
        }

        public record Message(string Body, string Title);
    }
}