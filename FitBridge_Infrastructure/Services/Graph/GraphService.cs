using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Graph.Entities;
using FitBridge_Domain.Graph.Entities.Relationships;
using OpenAI;
using OpenAI.Models;

namespace FitBridge_Infrastructure.Services.Graph
{
    internal class GraphService(IGraphRepository graphRepository) : IGraphService
    {
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