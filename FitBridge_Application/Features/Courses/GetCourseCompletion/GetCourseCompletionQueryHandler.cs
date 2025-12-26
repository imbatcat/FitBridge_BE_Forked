using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.Orders.GetOrderItemById;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.Courses.GetCourseCompletion
{
    internal class GetCourseCompletionQueryHandler(
        IUnitOfWork unitOfWork,
        ICourseCompletionService courseCompletionService)
        : IRequestHandler<GetCourseCompletionQuery, CourseCompletionResult?>
    {
        public async Task<CourseCompletionResult?> Handle(GetCourseCompletionQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetOrderItemByIdSpec(request.OrderItemId,
                isIncludeFreelancePackage: true,
                isIncludeGymCourse: true,
                isIncludeProduct: true);
            var orderItem = await unitOfWork.Repository<OrderItem>()
                .GetBySpecificationAsync(spec)
                ?? throw new NotFoundException(nameof(OrderItem));

            if (orderItem.ProductDetailId != null)
            {
                return null;
            }

            return await courseCompletionService.GetCourseCompletionAsync(request.OrderItemId);
        }
    }
}