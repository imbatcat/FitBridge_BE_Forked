using System;
using FitBridge_Application.Specifications.Reviews.GetMerchantReview;
using MediatR;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Reviews;

namespace FitBridge_Application.Features.Reviews.GetMerchantReview;

public class GetMerchantReviewQuery(GetMerchantReviewParams parameters) : IRequest<PagingResultDto<UserReviewResponseDto>>
{
    public GetMerchantReviewParams Params { get; set; } = parameters;
}
