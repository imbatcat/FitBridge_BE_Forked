using System;
using FitBridge_Application.Dtos.GymAssets;
using FitBridge_Application.Specifications.GymAssets.GetAssetMetadatForSessionActivity;
using MediatR;

namespace FitBridge_Application.Features.GymAssets.GetAssetMetadatForSessionActivity;

public class GetAssetMetadatForSessionActivityQuery(GetAssetMetadatForSessionActivityParams activityParams) : IRequest<List<SessionActivityAssetDto>>
{
    public GetAssetMetadatForSessionActivityParams Params { get; set; } = activityParams;
}
