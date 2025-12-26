using FitBridge_Domain.Graph.Entities;
using FitBridge_Domain.Graph.Entities.Relationships;
using System.Runtime.CompilerServices;

namespace FitBridge_Application.Interfaces.Services
{
    public interface IGraphService
    {
        public Task CreateNode(BaseNode node);

        public Task UpdateNode(BaseNode node);

        public Task DeleteNode(BaseNode node);

        public Task CreateRelationship(BaseRelationship relationship);

        public Task UpdateRelationship(BaseRelationship relationship);

        public Task DeleteRelationship(BaseRelationship relationship);

        public Task SyncFreelancePTCheapestCourseAsync(Guid ptId, CancellationToken cancellationToken = default);

        public Task SyncGymCheapestCourseAsync(Guid gymOwnerId, CancellationToken cancellationToken = default);

        public Task SyncFreelancePTReviewStatsAsync(Guid ptId, CancellationToken cancellationToken = default);

        public Task SyncGymReviewStatsAsync(Guid gymId, CancellationToken cancellationToken = default);
    }
}