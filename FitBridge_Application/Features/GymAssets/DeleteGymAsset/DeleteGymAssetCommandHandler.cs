using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.GymAssets.GetGymAssetById;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Exceptions;
using FitBridge_Domain.Graph.Entities.Relationships;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.GymAssets.DeleteGymAsset;

public class DeleteGymAssetCommandHandler(
    ILogger<DeleteGymAssetCommandHandler> logger,
    IGraphService graphService,
    IUnitOfWork unitOfWork,
    IUploadService uploadService,
    IApplicationUserService applicationUserService)
    : IRequestHandler<DeleteGymAssetCommand, bool>
{
    public async Task<bool> Handle(DeleteGymAssetCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetGymAssetByIdSpec(request.GymAssetId);
        var gymAsset = await unitOfWork.Repository<GymAsset>()
            .GetBySpecificationAsync(spec, asNoTracking: false);

        if (gymAsset == null)
        {
            throw new NotFoundException(nameof(GymAsset), request.GymAssetId);
        }

        if (gymAsset.ImageUrls != null && gymAsset.ImageUrls.Any())
        {
            foreach (var imageUrl in gymAsset.ImageUrls)
            {
                try
                {
                    await uploadService.DeleteFileAsync(imageUrl);
                }
                catch (Exception)
                {
                }
            }
        }

        unitOfWork.Repository<GymAsset>().Delete(gymAsset);
        await unitOfWork.CommitAsync();

        // Delete OWNS relationship after commit (non-blocking)
        try
        {
            var ownsRelationship = new OwnsRelationship
            {
                GymOwnerId = gymAsset.GymOwnerId.ToString(),
                GymAssetId = gymAsset.Id.ToString()
            };

            await graphService.DeleteRelationship(ownsRelationship);
            logger.LogInformation("Deleted OWNS relationship for GymAsset {GymAssetId}", gymAsset.Id);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete OWNS relationship for GymAsset {GymAssetId}: {ErrorMessage}", gymAsset.Id, ex.Message);
        }

        return true;
    }
}