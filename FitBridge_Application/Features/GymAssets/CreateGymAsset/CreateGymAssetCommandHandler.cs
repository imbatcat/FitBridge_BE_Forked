using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Exceptions;
using FitBridge_Domain.Graph.Entities.Relationships;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.GymAssets.CreateGymAsset;

public class CreateGymAssetCommandHandler(
    ILogger<CreateGymAssetCommandHandler> logger,
    IGraphService graphService,
    IUnitOfWork unitOfWork,
    IUploadService uploadService,
    IApplicationUserService applicationUserService)
    : IRequestHandler<CreateGymAssetCommand, Guid>
{
    public async Task<Guid> Handle(CreateGymAssetCommand request, CancellationToken cancellationToken)
    {
        var assetMetadata = await unitOfWork.Repository<AssetMetadata>().GetByIdAsync(request.AssetMetadataId);
        if (assetMetadata == null)
        {
            throw new NotFoundException(nameof(AssetMetadata), request.AssetMetadataId);
        }

        var gymOwner = await applicationUserService.GetByIdAsync(request.GymOwnerId, includes: new List<string> { "GymAssets" });
        if (gymOwner == null)
        {
            throw new NotFoundException("Gym owner not found");
        }
        if (gymOwner.GymAssets.Any(x => x.AssetMetadataId == request.AssetMetadataId))
        {
            throw new DataValidationFailedException("Gym owner already has this asset");
        }

        if (request.Quantity <= 0)
        {
            throw new DataValidationFailedException("Quantity must be greater than 0");
        }

        var gymAsset = new GymAsset
        {
            GymOwnerId = request.GymOwnerId,
            AssetMetadataId = request.AssetMetadataId,
            Quantity = request.Quantity,
            ImageUrls = new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.ImagesToAdd != null && request.ImagesToAdd.Any())
        {
            foreach (var file in request.ImagesToAdd)
            {
                var uploadedUrl = await uploadService.UploadFileAsync(file);
                gymAsset.ImageUrls.Add(uploadedUrl);
            }
        }

        unitOfWork.Repository<GymAsset>().Insert(gymAsset);
        await unitOfWork.CommitAsync();

        // Create OWNS relationship after commit (non-blocking)
        try
        {
            var ownsRelationship = new OwnsRelationship
            {
                GymOwnerId = request.GymOwnerId.ToString(),
                GymAssetId = gymAsset.Id.ToString()
            };

            await graphService.CreateRelationship(ownsRelationship);
            logger.LogInformation("Created OWNS relationship for GymAsset {GymAssetId}", gymAsset.Id);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create OWNS relationship for GymAsset {GymAssetId}: {ErrorMessage}", gymAsset.Id, ex.Message);
        }

        return gymAsset.Id;
    }
}