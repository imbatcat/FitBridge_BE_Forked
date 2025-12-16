using System;
using FitBridge_Domain.Enums.Gyms;
using FitBridge_Domain.Enums.SessionActivities;

namespace FitBridge_Application.Specifications.GymAssets.GetAssetMetadatForSessionActivity;

public class GetAssetMetadatForSessionActivityParams : BaseParams
{
    public Guid? AssetId { get; set; }
    public AssetType? AssetType { get; set; }
    public MuscleGroupEnum? MuscleGroup { get; set; }
}
