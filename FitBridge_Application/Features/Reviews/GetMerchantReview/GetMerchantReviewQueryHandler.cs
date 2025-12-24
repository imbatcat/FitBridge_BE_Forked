using System;
using FitBridge_Application.Interfaces.Repositories;
using MediatR;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Reviews;
using FitBridge_Application.Specifications.Reviews.GetMerchantReview;
using AutoMapper;
using FitBridge_Domain.Entities.MessageAndReview;
using FitBridge_Domain.Enums.Reviews;
using FitBridge_Application.Dtos.Gym;
using FitBridge_Application.Dtos.FreelancePTPackages;
using FitBridge_Application.Specifications.Reviews.GetAllReviewForCustomer;

namespace FitBridge_Application.Features.Reviews.GetMerchantReview;

public class GetMerchantReviewQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetMerchantReviewQuery, PagingResultDto<UserReviewResponseDto>>
{
    public async Task<PagingResultDto<UserReviewResponseDto>> Handle(GetMerchantReviewQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _unitOfWork.Repository<Review>().GetAllWithSpecificationAsync(new GetMerchantReviewSpec(request.Params));
        var reviewDtos = new List<UserReviewResponseDto>();
        foreach (var review in reviews)
        {
            var reviewDto = _mapper.Map<UserReviewResponseDto>(review);
        }
        return new PagingResultDto<UserReviewResponseDto>(reviews.Count, reviewDtos);
    }

}
