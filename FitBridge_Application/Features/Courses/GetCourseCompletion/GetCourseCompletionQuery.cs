using FitBridge_Application.Interfaces.Services;
using MediatR;

namespace FitBridge_Application.Features.Courses.GetCourseCompletion
{
    public class GetCourseCompletionQuery : IRequest<CourseCompletionResult>
    {
        public Guid OrderItemId { get; set; }
    }
}
