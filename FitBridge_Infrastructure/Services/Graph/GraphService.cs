using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.FreelancePtPackages.GetFreelancePTPackagesByPtId;
using FitBridge_Application.Specifications.GymCourses.GetGymCoursesByGymOwnerId;
using FitBridge_Application.Specifications.Reviews.GetReviewsByFreelancePtId;
using FitBridge_Application.Specifications.Reviews.GetReviewsByGymId;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Entities.MessageAndReview;
using FitBridge_Domain.Graph.Entities;
using FitBridge_Domain.Graph.Entities.Relationships;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Models;

namespace FitBridge_Infrastructure.Services.Graph
{
    internal class GraphService : IGraphService
    {
        private readonly IGraphRepository graphRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public GraphService(
            IGraphRepository graphRepository, 
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            this.graphRepository = graphRepository;
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        public async Task CreateNode(BaseNode node)
        {
            switch (node)
            {
                case FreelancePTNode fpt:
                    await graphRepository.CreateFreelancePTNodeAsync(fpt);
                    break;

                case CertificateNode cert:
                    var embeddingText = GetCertificateEmbeddingText(cert);
                    using (var openAiClient = new OpenAIClient())
                    {
                        var response = await openAiClient.EmbeddingsEndpoint.CreateEmbeddingAsync(embeddingText, Model.Embedding_3_Small);
                        cert.Embedding = response.Data[0].Embedding.Select(d => (float)d).ToList();
                    }
                    await graphRepository.CreateCertificateNodeAsync(cert);
                    break;

                case GymAssetNode asset:
                    var assetEmbeddingText = GetGymAssetEmbeddingText(asset);
                    using (var openAiClient = new OpenAIClient())
                    {
                        var response = await openAiClient.EmbeddingsEndpoint.CreateEmbeddingAsync(assetEmbeddingText, Model.Embedding_3_Small);
                        asset.Embedding = response.Data[0].Embedding.Select(d => (float)d).ToList();
                    }
                    await graphRepository.CreateGymAssetNodeAsync(asset);
                    break;

                case GymNode gym:
                    await graphRepository.CreateGymNodeAsync(gym);
                    break;

                case MusclesNode muscle:
                    await graphRepository.CreateMuscleNodeAsync(muscle);
                    break;
            }
        }

        public async Task CreateRelationship(BaseRelationship relationship)
        {
            switch (relationship)
            {
                case HasCertificateRelationship hasCert:
                    await graphRepository.CreateHasCertificateRelationshipAsync(hasCert);
                    break;

                case OwnsRelationship owns:
                    await graphRepository.CreateOwnsRelationshipAsync(owns);
                    break;

                case TargetsRelationship targets:
                    await graphRepository.CreateTargetsRelationshipAsync(targets);
                    break;
            }
        }

        public async Task DeleteNode(BaseNode node)
        {
            switch (node)
            {
                case FreelancePTNode fpt:
                    await graphRepository.DeleteFreelancePTNodeAsync(fpt.DbId);
                    break;

                case CertificateNode cert:
                    await graphRepository.DeleteCertificateNodeAsync(cert.DbId);
                    break;

                case GymAssetNode asset:
                    await graphRepository.DeleteGymAssetNodeAsync(asset.DbId);
                    break;

                case GymNode gym:
                    await graphRepository.DeleteGymNodeAsync(gym.DbId);
                    break;

                case MusclesNode muscle:
                    await graphRepository.DeleteMuscleNodeAsync(muscle.Name);
                    break;
            }
        }

        public async Task DeleteRelationship(BaseRelationship relationship)
        {
            switch (relationship)
            {
                case HasCertificateRelationship hasCert:
                    await graphRepository.DeleteHasCertificateRelationshipAsync(hasCert);
                    break;

                case OwnsRelationship owns:
                    await graphRepository.DeleteOwnsRelationshipAsync(owns);
                    break;

                case TargetsRelationship targets:
                    await graphRepository.DeleteTargetsRelationshipAsync(targets);
                    break;
            }
        }

        public async Task UpdateNode(BaseNode node)
        {
            switch (node)
            {
                case FreelancePTNode fpt:
                    await graphRepository.UpdateFreelancePTNodeAsync(fpt);
                    break;

                case CertificateNode cert:
                    var embeddingText = GetCertificateEmbeddingText(cert);
                    using (var openAiClient = new OpenAIClient())
                    {
                        var response = await openAiClient.EmbeddingsEndpoint.CreateEmbeddingAsync(embeddingText, Model.Embedding_3_Small);
                        cert.Embedding = response.Data[0].Embedding.Select(d => (float)d).ToList();
                    }
                    await graphRepository.UpdateCertificateNodeAsync(cert);
                    break;

                case GymAssetNode asset:
                    var assetEmbeddingText = GetGymAssetEmbeddingText(asset);
                    using (var openAiClient = new OpenAIClient())
                    {
                        var response = await openAiClient.EmbeddingsEndpoint.CreateEmbeddingAsync(assetEmbeddingText, Model.Embedding_3_Small);
                        asset.Embedding = response.Data[0].Embedding.Select(d => (float)d).ToList();
                    }
                    await graphRepository.UpdateGymAssetNodeAsync(asset);
                    break;

                case GymNode gym:
                    await graphRepository.UpdateGymNodeAsync(gym);
                    break;

                case MusclesNode:
                    throw new NotSupportedException("Updating MusclesNode is not supported via generic UpdateNode as it requires the old name.");
            }
        }

        public async Task UpdateRelationship(BaseRelationship relationship)
        {
            switch (relationship)
            {
                case HasCertificateRelationship hasCert:
                    await graphRepository.UpdateHasCertificateRelationshipAsync(hasCert);
                    break;

                case OwnsRelationship owns:
                    await graphRepository.UpdateOwnsRelationshipAsync(owns);
                    break;

                case TargetsRelationship targets:
                    await graphRepository.UpdateTargetsRelationshipAsync(targets);
                    break;
            }
        }

        public async Task SyncFreelancePTCheapestCourseAsync(Guid ptId, CancellationToken cancellationToken = default)
        {
            // Get all packages for this PT from PostgreSQL using specification
            var specification = new GetFreelancePTPackagesByPtIdSpec(ptId);
            var packages = await unitOfWork.Repository<FreelancePTPackage>()
                .GetAllWithSpecificationAsync(specification, asNoTracking: true);

            // Get the existing FreelancePTNode from Neo4j
            var existingNode = await graphRepository.GetFreelancePTByIdAsync(ptId.ToString(), cancellationToken);
            
            if (existingNode == null)
            {
                // Node doesn't exist, skip sync
                return;
            }

            if (packages.Any())
            {
                var cheapestPackage = packages.First();
                
                // Update the node with cheapest course info
                existingNode.CheapestCourse = cheapestPackage.Name;
                existingNode.CheapestPrice = cheapestPackage.Price;
                existingNode.CourseDescription = cheapestPackage.Description ?? string.Empty;
                existingNode.FreelancePtCourseId = cheapestPackage.Id.ToString();
            }
            else
            {
                // No packages, clear the cheapest course info
                existingNode.CheapestCourse = string.Empty;
                existingNode.CheapestPrice = 0;
                existingNode.CourseDescription = string.Empty;
                existingNode.FreelancePtCourseId = string.Empty;
            }

            // Update the node in Neo4j
            await graphRepository.UpdateFreelancePTNodeAsync(existingNode, cancellationToken);
        }

        public async Task SyncGymCheapestCourseAsync(Guid gymOwnerId, CancellationToken cancellationToken = default)
        {
            // Get all courses for this gym from PostgreSQL using specification
            var specification = new GetGymCoursesByGymOwnerIdSpec(gymOwnerId);
            var courses = await unitOfWork.Repository<GymCourse>()
                .GetAllWithSpecificationAsync(specification, asNoTracking: true);

            // Get the existing GymNode from Neo4j
            var existingNode = await graphRepository.GetGymByIdAsync(gymOwnerId.ToString(), cancellationToken);
            
            if (existingNode == null)
            {
                // Node doesn't exist, skip sync
                return;
            }

            if (courses.Any())
            {
                var cheapestCourse = courses.First();
                
                // Update the node with cheapest course info
                existingNode.CheapestCourse = cheapestCourse.Name;
                existingNode.CheapestPrice = cheapestCourse.Price;
                existingNode.CourseId = cheapestCourse.Id.ToString();
            }
            else
            {
                // No courses, clear the cheapest course info
                existingNode.CheapestCourse = string.Empty;
                existingNode.CheapestPrice = 0;
                existingNode.CourseId = string.Empty;
            }

            // Update the node in Neo4j
            await graphRepository.UpdateGymNodeAsync(existingNode, cancellationToken);
        }

        public async Task SyncFreelancePTReviewStatsAsync(Guid ptId, CancellationToken cancellationToken = default)
        {
            // Get all reviews for this PT from PostgreSQL using specification
            var specification = new GetReviewsByFreelancePtIdSpec(ptId);
            var reviews = await unitOfWork.Repository<Review>()
                .GetAllWithSpecificationAsync(specification, asNoTracking: true);

            // Get the existing FreelancePTNode from Neo4j
            var existingNode = await graphRepository.GetFreelancePTByIdAsync(ptId.ToString(), cancellationToken);
            
            if (existingNode == null)
            {
                return;
            }

            if (reviews.Any())
            {
                var avgRating = reviews.Average(r => r.Rating);
                existingNode.AverageRating = avgRating;
                existingNode.ReviewCount = reviews.Count;
            }
            else
            {
                existingNode.AverageRating = 0;
                existingNode.ReviewCount = 0;
            }

            await graphRepository.UpdateFreelancePTNodeAsync(existingNode, cancellationToken);
        }

        public async Task SyncGymReviewStatsAsync(Guid gymId, CancellationToken cancellationToken = default)
        {
            // Get all reviews for this gym from PostgreSQL using specification
            var specification = new GetReviewsByGymIdSpec(gymId);
            var reviews = await unitOfWork.Repository<Review>()
                .GetAllWithSpecificationAsync(specification, asNoTracking: true);

            // Get the existing GymNode from Neo4j
            var existingNode = await graphRepository.GetGymByIdAsync(gymId.ToString(), cancellationToken);
            
            if (existingNode == null)
            {
                return;
            }

            if (reviews.Any())
            {
                var avgRating = reviews.Average(r => r.Rating);
                existingNode.AverageRating = avgRating;
            }
            else
            {
                existingNode.AverageRating = 0;
            }

            await graphRepository.UpdateGymNodeAsync(existingNode, cancellationToken);
        }

        private static string GetCertificateEmbeddingText(CertificateNode node)
        {
            return $"Certificate Name: {node.CertName}, Description: {node.Description}, Type: {node.CertificateType}, Provider: {node.ProviderName}, Code: {node.CertCode}";
        }

        private static string GetGymAssetEmbeddingText(GymAssetNode node)
        {
            return $"Name: {node.Name}, Description: {node.Description}, Category: {node.AssetType}";
        }
    }
}