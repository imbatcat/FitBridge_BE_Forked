using System;
using FitBridge_Application.Interfaces.Repositories;
using MediatR;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Reviews;
using FitBridge_Application.Specifications.Reviews.GetAllReview;
using FitBridge_Domain.Entities.MessageAndReview;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Exceptions;
using FitBridge_Domain.Entities.Ecommerce;
using AutoMapper;

namespace FitBridge_Application.Features.Reviews.GetAllReview;

public class GetAllReviewByTargetIdQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetAllReviewByTargetIdQuery, PagingResultDto<ReviewProductResponseDto>>
{
    public async Task<PagingResultDto<ReviewProductResponseDto>> Handle(GetAllReviewByTargetIdQuery request, CancellationToken cancellationToken)
    {
        var (targetId, spec) = await DetermineTargetId(request);
        var reviews = await _unitOfWork.Repository<Review>().GetAllWithSpecificationAsync(spec);
        var totalItems = await _unitOfWork.Repository<Review>().CountAsync(spec);
        var dtos = _mapper.Map<List<ReviewProductResponseDto>>(reviews);
        return new PagingResultDto<ReviewProductResponseDto>(totalItems, dtos);
    }

    public async Task<(Guid, GetAllReviewByTargetIdSpec)> DetermineTargetId(GetAllReviewByTargetIdQuery request)
    {
        var gymOwnerId = request.Params.GymOwnerId;
        var freelancePtId = request.Params.FreelancePtId;
        var productId = request.Params.ProductId;
        if(freelancePtId != null)
        {

            return (freelancePtId.Value, new GetAllReviewByTargetIdSpec(request.Params, FreelancePtId: freelancePtId.Value));
        }
        if(productId != null)
        {
            return (productId.Value, new GetAllReviewByTargetIdSpec(request.Params, ProductId: productId.Value));
        }
        if(gymOwnerId != null)
        {
            return (gymOwnerId.Value, new GetAllReviewByTargetIdSpec(request.Params, GymId: gymOwnerId.Value));
        }
        throw new NotFoundException("Target not found");
    }
}
