using AutoMapper;
using FitBridge_Application.Dtos.GymCourses;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Entities.Gyms;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.GymCourses.CreateGymCourse
{
    internal class CreateGymCourseCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IGraphService graphService,
        ILogger<CreateGymCourseCommandHandler> logger) : IRequestHandler<CreateGymCourseCommand, CreateGymCourseResponse>
    {
        public async Task<CreateGymCourseResponse> Handle(CreateGymCourseCommand request, CancellationToken cancellationToken)
        {
            var mappedEntity = mapper.Map<CreateGymCourseCommand, GymCourse>(request);
            var newId = Guid.NewGuid();
            mappedEntity.Id = newId;
            mappedEntity.GymOwnerId = Guid.Parse(request.GymOwnerId);

            unitOfWork.Repository<GymCourse>().Insert(mappedEntity);
            await unitOfWork.CommitAsync();

            try
            {
                await graphService.SyncGymCheapestCourseAsync(mappedEntity.GymOwnerId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync Gym cheapest course to Neo4j for Gym {GymOwnerId}", mappedEntity.GymOwnerId);
            }

            return mappedEntity is not null
                ? new CreateGymCourseResponse
                {
                    Id = mappedEntity.Id,
                    GymOwnerId = mappedEntity.GymOwnerId,
                    Name = mappedEntity.Name,
                    Description = mappedEntity.Description,
                    Price = mappedEntity.Price,
                    Duration = mappedEntity.Duration,
                    Type = mappedEntity.Type,
                    ImageUrl = mappedEntity.ImageUrl,
                    PtPrice = mappedEntity.PtPrice
                }
                : null!;
        }
    }
}