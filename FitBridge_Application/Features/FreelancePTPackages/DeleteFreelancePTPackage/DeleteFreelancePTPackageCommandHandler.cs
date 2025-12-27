using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.FreelancePtPackages.GetFreelancePtPackageById;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.FreelancePTPackages.DeleteFreelancePTPackage
{
    internal class DeleteFreelancePTPackageCommandHandler(
        IUnitOfWork unitOfWork,
        IGraphService graphService,
        ILogger<DeleteFreelancePTPackageCommandHandler> logger) : IRequestHandler<DeleteFreelancePTPackageCommand>
    {
        public async Task Handle(DeleteFreelancePTPackageCommand request, CancellationToken cancellationToken)
        {
            var spec = new GetFreelancePtPackageByIdSpec(request.PackageId);
            var existingPackage = await unitOfWork.Repository<FreelancePTPackage>()
                .GetBySpecificationAsync(spec, asNoTracking: false)
                ?? throw new NotFoundException(nameof(FreelancePTPackage));

            unitOfWork.Repository<FreelancePTPackage>().SoftDelete(existingPackage);

            await unitOfWork.CommitAsync();

            try
            {
                var ptId = existingPackage.PtId;
                await graphService.SyncFreelancePTCheapestCourseAsync(ptId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync FreelancePT cheapest course to Neo4j for package {PackageId}", request.PackageId);
            }
        }
    }
}