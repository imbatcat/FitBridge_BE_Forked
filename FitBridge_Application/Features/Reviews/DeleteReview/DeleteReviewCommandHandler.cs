using System;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Entities.MessageAndReview;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.Reviews.DeleteReview;

public class DeleteReviewCommandHandler(
    IUnitOfWork _unitOfWork,
    IGraphService graphService,
    ILogger<DeleteReviewCommandHandler> logger) : IRequestHandler<DeleteReviewCommand, bool>
{
    public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Repository<Review>().GetByIdAsync(request.ReviewId);
        if (review == null)
        {
            throw new NotFoundException($"Review {request.ReviewId} not found");
        }

        _unitOfWork.Repository<Review>().Delete(review);
        await _unitOfWork.CommitAsync();

        try
        {
            var freelancePtId = review.FreelancePtId;
            var gymId = review.GymId;
            if (freelancePtId.HasValue)
            {
                await graphService.SyncFreelancePTReviewStatsAsync(freelancePtId.Value, cancellationToken);
            }
            else if (gymId.HasValue)
            {
                await graphService.SyncGymReviewStatsAsync(gymId.Value, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync review stats to Neo4j for review {ReviewId}", request.ReviewId);
        }

        return true;
    }
}