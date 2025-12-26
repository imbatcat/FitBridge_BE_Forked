using AutoMapper;
using FitBridge_Application.Dtos.FreelancePTPackages;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Domain.Entities.Gyms;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.FreelancePTPackages.CreateFreelancePTPackage
{
    internal class CreateFreelancePTPackageCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IUserUtil userUtil,
        IHttpContextAccessor httpContextAccessor,
        IGraphService graphService,
        ILogger<CreateFreelancePTPackageCommandHandler> logger) : IRequestHandler<CreateFreelancePTPackageCommand, CreateFreelancePTPackageDto>
    {
        public async Task<CreateFreelancePTPackageDto> Handle(CreateFreelancePTPackageCommand request, CancellationToken cancellationToken)
        {
            var accountId = userUtil.GetAccountId(httpContextAccessor.HttpContext);
            var newPackage = new FreelancePTPackage
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                DurationInDays = request.DurationInDays,
                SessionDurationInMinutes = request.SessionDurationInMinutes,
                NumOfSessions = request.NumOfSessions,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                PtId = accountId!.Value,
                IsDisplayed = request.IsDisplayed
            };

            unitOfWork.Repository<FreelancePTPackage>().Insert(newPackage);

            await unitOfWork.CommitAsync();

            try
            {
                await graphService.SyncFreelancePTCheapestCourseAsync(accountId.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync FreelancePT cheapest course to Neo4j for PT {PtId}", accountId.Value);
            }

            var dto = mapper.Map<CreateFreelancePTPackageDto>(newPackage);
            return dto;
        }
    }
}