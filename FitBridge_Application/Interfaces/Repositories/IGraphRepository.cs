using FitBridge_Domain.Graph.Entities;
using FitBridge_Domain.Graph.Entities.Relationships;

namespace FitBridge_Application.Interfaces.Repositories
{
    public interface IGraphRepository
    {
        // FreelancePT Node Operations
        Task<FreelancePTNode?> GetFreelancePTByIdAsync(string dbId, CancellationToken cancellationToken = default);

        Task<IEnumerable<FreelancePTNode>> GetAllFreelancePTsAsync(CancellationToken cancellationToken = default);

        Task CreateFreelancePTNodeAsync(FreelancePTNode node, CancellationToken cancellationToken = default);

        Task UpdateFreelancePTNodeAsync(FreelancePTNode node, CancellationToken cancellationToken = default);

        Task DeleteFreelancePTNodeAsync(string dbId, CancellationToken cancellationToken = default);

        // Certificate Node Operations
        Task<CertificateNode?> GetCertificateByIdAsync(string dbId, CancellationToken cancellationToken = default);

        Task<IEnumerable<CertificateNode>> GetAllCertificatesAsync(CancellationToken cancellationToken = default);

        Task CreateCertificateNodeAsync(CertificateNode node, CancellationToken cancellationToken = default);

        Task UpdateCertificateNodeAsync(CertificateNode node, CancellationToken cancellationToken = default);

        Task DeleteCertificateNodeAsync(string dbId, CancellationToken cancellationToken = default);

        // Gym Node Operations
        Task<GymNode?> GetGymByIdAsync(string dbId, CancellationToken cancellationToken = default);

        Task<IEnumerable<GymNode>> GetAllGymsAsync(CancellationToken cancellationToken = default);

        Task CreateGymNodeAsync(GymNode node, CancellationToken cancellationToken = default);

        Task UpdateGymNodeAsync(GymNode node, CancellationToken cancellationToken = default);

        Task DeleteGymNodeAsync(string dbId, CancellationToken cancellationToken = default);

        // GymAsset Node Operations
        Task<GymAssetNode?> GetGymAssetByIdAsync(string dbId, CancellationToken cancellationToken = default);

        Task<IEnumerable<GymAssetNode>> GetAllGymAssetsAsync(CancellationToken cancellationToken = default);

        Task CreateGymAssetNodeAsync(GymAssetNode node, CancellationToken cancellationToken = default);

        Task UpdateGymAssetNodeAsync(GymAssetNode node, CancellationToken cancellationToken = default);

        Task DeleteGymAssetNodeAsync(string dbId, CancellationToken cancellationToken = default);

        // Muscles Node Operations
        Task<MusclesNode?> GetMuscleByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<IEnumerable<MusclesNode>> GetAllMusclesAsync(CancellationToken cancellationToken = default);

        Task CreateMuscleNodeAsync(MusclesNode node, CancellationToken cancellationToken = default);

        Task UpdateMuscleNodeAsync(string oldName, MusclesNode node, CancellationToken cancellationToken = default);

        Task DeleteMuscleNodeAsync(string name, CancellationToken cancellationToken = default);

        // Relationship Operations - HasCertificate
        Task CreateHasCertificateRelationshipAsync(HasCertificateRelationship relationship, CancellationToken cancellationToken = default);

        Task UpdateHasCertificateRelationshipAsync(HasCertificateRelationship relationship, CancellationToken cancellationToken = default);

        Task DeleteHasCertificateRelationshipAsync(HasCertificateRelationship relationship, CancellationToken cancellationToken = default);

        Task<IEnumerable<CertificateNode>> GetCertificatesForFreelancePTAsync(string freelancePTId, CancellationToken cancellationToken = default);

        // Relationship Operations - Owns
        Task CreateOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default);

        Task UpdateOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default);

        Task DeleteOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default);

        Task<IEnumerable<GymNode>> GetGymsOwnedByAsync(string gymOwnerId, CancellationToken cancellationToken = default);

        // Relationship Operations - Targets
        Task CreateTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default);

        Task UpdateTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default);

        Task DeleteTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default);

        Task<IEnumerable<MusclesNode>> GetTargetedMusclesAsync(string sourceId, CancellationToken cancellationToken = default);

        // Advanced Query Operations
        Task<IEnumerable<FreelancePTNode>> GetNearbyFreelancePTsAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default);

        Task<IEnumerable<GymNode>> GetNearbyGymsAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default);

        Task<IEnumerable<FreelancePTNode>> GetFreelancePTsBySpecializationAsync(string specialization, CancellationToken cancellationToken = default);

        Task<IEnumerable<GymNode>> GetGymsByRatingAsync(double minRating, CancellationToken cancellationToken = default);

        Task<IEnumerable<GymAssetNode>> GetAssetsForGymAsync(string gymId, CancellationToken cancellationToken = default);
    }
}