using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Gyms;

namespace FitBridge_Application.Specifications.GymAssets.GetGymAssetById;

public class GetGymAssetByIdSpec : BaseSpecification<GymAsset>
{
    public GetGymAssetByIdSpec(Guid gymAssetId)
        : base(x => x.AssetMetadataId == gymAssetId)
    {
        AddInclude(x => x.GymOwner);
        AddInclude(x => x.AssetMetadata);
    }
}