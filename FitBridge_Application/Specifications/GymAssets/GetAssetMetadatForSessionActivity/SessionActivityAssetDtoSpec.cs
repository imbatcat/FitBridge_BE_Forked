using System;
using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Gyms;

namespace FitBridge_Application.Specifications.GymAssets.GetAssetMetadatForSessionActivity;

public class SessionActivityAssetDtoSpec : BaseSpecification<AssetMetadata>
{
    public SessionActivityAssetDtoSpec(GetAssetMetadatForSessionActivityParams parameters) : base(x =>
    (parameters.AssetId == null || x.Id == parameters.AssetId)
    && (parameters.AssetType == null || x.AssetType == parameters.AssetType)
    && (parameters.SearchTerm == null || x.Name.ToLower().Contains(parameters.SearchTerm.ToLower())
    || x.VietNameseName.ToLower().Contains(parameters.SearchTerm.ToLower()))
    && (parameters.MuscleGroup == null || x.TargetMuscularGroups.Contains(parameters.MuscleGroup.ToString()))
    )
    {
        if(parameters.SortOrder == "asc") {
            AddOrderBy(x => x.Name);
        } else {
            AddOrderByDesc(x => x.Name);
        }
        
        if (parameters.DoApplyPaging)
        {
            AddPaging(parameters.Size * (parameters.Page - 1), parameters.Size);
        }
        else
        {
            parameters.Size = -1;
            parameters.Page = -1;
        }
    }
}
