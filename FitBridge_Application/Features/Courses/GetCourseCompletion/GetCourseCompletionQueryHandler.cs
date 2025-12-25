using FitBridge_Application.Interfaces.Services;
using MediatR;

namespace FitBridge_Application.Features.Courses.GetCourseCompletion
{
    internal class GetCourseCompletionQueryHandler(ICourseCompletionService courseCompletionService)
        : IRequestHandler<GetCourseCompletionQuery, CourseCompletionResult>
    {
        public async Task<CourseCompletionResult> Handle(GetCourseCompletionQuery request, CancellationToken cancellationToken)
        {
            return await courseCompletionService.GetCourseCompletionAsync(request.OrderItemId);
        }
    }
}
