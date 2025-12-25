using FitBridge_Domain.Graph.Entities;
using FitBridge_Domain.Graph.Entities.Relationships;

namespace FitBridge_Application.Interfaces.Repositories
{
    public interface IGraphRepository
    {
        // FreelancePT Node Operations
        Task CreateFreelancePTNodeAsync(FreelancePTNode node, CancellationToken cancellationToken = default);

        Task UpdateFreelancePTNodeAsync(FreelancePTNode node, CancellationToken cancellationToken = default);

        Task DeleteFreelancePTNodeAsync(string dbId, CancellationToken cancellationToken = default);

        // Certificate Node Operations
        Task CreateCertificateNodeAsync(CertificateNode node, CancellationToken cancellationToken = default);

        Task UpdateCertificateNodeAsync(CertificateNode node, CancellationToken cancellationToken = default);

        Task DeleteCertificateNodeAsync(string dbId, CancellationToken cancellationToken = default);

        // Gym Node Operations
        Task CreateGymNodeAsync(GymNode node, CancellationToken cancellationToken = default);

        Task UpdateGymNodeAsync(GymNode node, CancellationToken cancellationToken = default);

        Task DeleteGymNodeAsync(string dbId, CancellationToken cancellationToken = default);

        // GymAsset Node Operations
        Task CreateGymAssetNodeAsync(GymAssetNode node, CancellationToken cancellationToken = default);

        Task UpdateGymAssetNodeAsync(GymAssetNode node, CancellationToken cancellationToken = default);

        Task DeleteGymAssetNodeAsync(string dbId, CancellationToken cancellationToken = default);

        // Muscles Node Operations
        Task CreateMuscleNodeAsync(MusclesNode node, CancellationToken cancellationToken = default);

        Task UpdateMuscleNodeAsync(string oldName, MusclesNode node, CancellationToken cancellationToken = default);

        Task DeleteMuscleNodeAsync(string name, CancellationToken cancellationToken = default);

        // Relationship Operations - HasCertificate
        Task CreateHasCertificateRelationshipAsync(HasCertificateRelationship relationship, CancellationToken cancellationToken = default);

        Task UpdateHasCertificateRelationshipAsync(HasCertificateRelationship relationship, CancellationToken cancellationToken = default);

        Task DeleteHasCertificateRelationshipAsync(string freelancePTId, string certificateId, CancellationToken cancellationToken = default);

        // Relationship Operations - Owns
        Task CreateOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default);

        Task UpdateOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default);

        Task DeleteOwnsRelationshipAsync(string gymOwnerId, string gymId, CancellationToken cancellationToken = default);

        // Relationship Operations - Targets
        Task CreateTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default);

        Task UpdateTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default);

        Task DeleteTargetsRelationshipAsync(string sourceId, string muscleId, CancellationToken cancellationToken = default);
    }
}