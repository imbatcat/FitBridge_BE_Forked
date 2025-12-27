using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.GymCourses.GetGymCourseById;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.GymCourses.DeleteGymCourseById
{
    public class DeleteGymCourseByIdCommandHandler(
        IUnitOfWork unitOfWork,
        IGraphService graphService,
        ILogger<DeleteGymCourseByIdCommandHandler> logger) : IRequestHandler<DeleteGymCourseByIdCommand>
    {
        async Task IRequestHandler<DeleteGymCourseByIdCommand>.Handle(DeleteGymCourseByIdCommand request, CancellationToken cancellationToken)
        {
            var spec = new GetGymCourseByIdSpecification(request.GymCourseId, includeGymOwner: false);
            var entity = await unitOfWork.Repository<GymCourse>().GetBySpecificationAsync(spec, asNoTracking: false) ?? throw new NotFoundException(nameof(GymCourse));

            unitOfWork.Repository<GymCourse>().SoftDelete(entity);

            await unitOfWork.CommitAsync();

            try
            {
                var gymOwnerId = entity.GymOwnerId;
                await graphService.SyncGymCheapestCourseAsync(gymOwnerId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync Gym cheapest course to Neo4j for course {GymCourseId}", request.GymCourseId);
            }
        }
    }
}