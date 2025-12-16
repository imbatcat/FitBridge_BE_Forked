using System;
using FitBridge_Application.Dtos.GymAssets;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Specifications.GymAssets.GetAssetMetadatForSessionActivity;
using FitBridge_Domain.Entities.Gyms;
using MediatR;
using AutoMapper;

namespace FitBridge_Application.Features.GymAssets.GetAssetMetadatForSessionActivity;

public class GetAssetMetadatForSessionActivityQueryHandler(IUnitOfWork _unitOfWork, IMapper mapper) : IRequestHandler<GetAssetMetadatForSessionActivityQuery, List<SessionActivityAssetDto>>
{
    public async Task<List<SessionActivityAssetDto>> Handle(GetAssetMetadatForSessionActivityQuery request, CancellationToken cancellationToken)
    {
        var spec = new SessionActivityAssetDtoSpec(request.Params);
        var assetMetadata = await _unitOfWork.Repository<AssetMetadata>().GetAllWithSpecificationAsync(spec);
        var result = mapper.Map<List<SessionActivityAssetDto>>(assetMetadata);
        return result;
    }
}
