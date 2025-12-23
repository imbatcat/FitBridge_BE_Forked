using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.Orders.GetOrderItemById;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Trainings;
using FitBridge_Domain.Exceptions;

namespace FitBridge_Application.Services
{
    public class CourseCompletionService(IUnitOfWork unitOfWork) : ICourseCompletionService
    {
        public async Task<CourseCompletionResult> GetCourseCompletionAsync(Guid orderItemId)
        {
            var spec = new GetOrderItemWithCourseCompletionSpec(orderItemId);
            var orderItem = await unitOfWork.Repository<OrderItem>()
                .GetBySpecificationAsync(spec, asNoTracking: true)
                ?? throw new NotFoundException(nameof(OrderItem));

            // Determine course type and get total sessions
            int totalSessions;
            string courseName;
            bool isGymCourse;

            if (orderItem.GymCourseId != null && orderItem.GymCourse != null)
            {
                isGymCourse = true;
                totalSessions = orderItem.GymCourse.Duration;
                courseName = orderItem.GymCourse.Name;
            }
            else if (orderItem.FreelancePTPackageId != null && orderItem.FreelancePTPackage != null)
            {
                isGymCourse = false;
                totalSessions = orderItem.FreelancePTPackage.NumOfSessions;
                courseName = orderItem.FreelancePTPackage.Name;
            }
            else
            {
                throw new DataValidationFailedException("OrderItem has neither GymCourse nor FreelancePTPackage");
            }

            if (orderItem.CustomerPurchasedId == null || orderItem.CustomerPurchased == null)
            {
                return new CourseCompletionResult
                {
                    OrderItemId = orderItemId,
                    CustomerPurchasedId = null,
                    CourseName = courseName,
                    IsGymCourse = isGymCourse,
                    TotalSessions = totalSessions,
                    CompletedSessions = 0,
                    CancelledSessions = 0,
                    UpcomingSessions = 0,
                    AvailableSessions = 0,
                    CompletionPercentage = 0m,
                    IsCompleted = false,
                    ExpirationDate = null
                };
            }

            var customerPurchased = orderItem.CustomerPurchased;
            var bookings = customerPurchased.Bookings.ToList();

            var completedSessions = bookings.Count(b => b.SessionStatus == SessionStatus.Finished);
            var cancelledSessions = bookings.Count(b => b.SessionStatus == SessionStatus.Cancelled);
            var upcomingSessions = bookings.Count(b => b.SessionStatus == SessionStatus.Booked);

            var completionPercentage = totalSessions > 0
                ? Math.Round((decimal)completedSessions / totalSessions * 100, 2)
                : 0m;

            return new CourseCompletionResult
            {
                OrderItemId = orderItemId,
                CustomerPurchasedId = customerPurchased.Id,
                CourseName = courseName,
                IsGymCourse = isGymCourse,
                TotalSessions = totalSessions,
                CompletedSessions = completedSessions,
                CancelledSessions = cancelledSessions,
                UpcomingSessions = upcomingSessions,
                AvailableSessions = customerPurchased.AvailableSessions,
                CompletionPercentage = completionPercentage,
                IsCompleted = completedSessions >= totalSessions,
                ExpirationDate = customerPurchased.ExpirationDate
            };
        }

        public async Task<bool> IsCourseCompletedAsync(Guid orderItemId)
        {
            var result = await GetCourseCompletionAsync(orderItemId);
            return result.IsCompleted;
        }
    }
}