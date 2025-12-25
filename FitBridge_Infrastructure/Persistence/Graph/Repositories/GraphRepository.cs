using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Graph.Entities;
using FitBridge_Domain.Graph.Entities.Relationships;
using Neo4j.Driver;

namespace FitBridge_Infrastructure.Persistence.Graph.Repositories
{
    public class GraphRepository : IGraphRepository
    {
        private readonly IDriver _driver;

        public GraphRepository(IDriver driver)
        {
            _driver = driver;
        }

        #region FreelancePT Node Operations

        public async Task CreateFreelancePTNodeAsync(FreelancePTNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"CREATE (f:FreelancePT {
                        dbId: $dbId,
                        fullName: $fullName,
                        email: $email,
                        phoneNumber: $phoneNumber,
                        isMale: $isMale,
                        dob: datetime($dob),
                        businessAddress: $businessAddress,
                        lat: $lat,
                        lon: $lon,
                        courseDescription: $courseDescription,
                        cheapestCourse: $cheapestCourse,
                        cheapestPrice: $cheapestPrice,
                        freelancePtCourseId: $freelancePtCourseId,
                        embedding: $embedding
                    })",
                    new
                    {
                        dbId = node.DbId,
                        fullName = node.FullName,
                        email = node.Email,
                        phoneNumber = node.PhoneNumber,
                        isMale = node.IsMale,
                        dob = node.DateOfBirth.ToString("o"),
                        businessAddress = node.BusinessAddress,
                        lat = node.Latitude,
                        lon = node.Longitude,
                        courseDescription = node.CourseDescription,
                        cheapestCourse = node.CheapestCourse,
                        cheapestPrice = node.CheapestPrice,
                        freelancePtCourseId = node.FreelancePtCourseId,
                        embedding = node.Embedding
                    });
            });
        }

        public async Task UpdateFreelancePTNodeAsync(FreelancePTNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (f:FreelancePT {dbId: $dbId})
                      SET f.fullName = $fullName,
                          f.email = $email,
                          f.phoneNumber = $phoneNumber,
                          f.isMale = $isMale,
                          f.dob = datetime($dob),
                          f.businessAddress = $businessAddress,
                          f.lat = $lat,
                          f.lon = $lon,
                          f.courseDescription = $courseDescription,
                          f.cheapestCourse = $cheapestCourse,
                          f.cheapestPrice = $cheapestPrice,
                          f.freelancePtCourseId = $freelancePtCourseId,
                          f.embedding = $embedding",
                    new
                    {
                        dbId = node.DbId,
                        fullName = node.FullName,
                        email = node.Email,
                        phoneNumber = node.PhoneNumber,
                        isMale = node.IsMale,
                        dob = node.DateOfBirth.ToString("o"),
                        businessAddress = node.BusinessAddress,
                        lat = node.Latitude,
                        lon = node.Longitude,
                        courseDescription = node.CourseDescription,
                        cheapestCourse = node.CheapestCourse,
                        cheapestPrice = node.CheapestPrice,
                        freelancePtCourseId = node.FreelancePtCourseId,
                        embedding = node.Embedding
                    });
            });
        }

        public async Task DeleteFreelancePTNodeAsync(string dbId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (f:FreelancePT {dbId: $dbId})
                      DETACH DELETE f",
                    new { dbId });
            });
        }

        #endregion FreelancePT Node Operations

        #region Certificate Node Operations

        public async Task CreateCertificateNodeAsync(CertificateNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"CREATE (c:Certificates {
                        dbId: $dbId,
                        certCode: $certCode,
                        certName: $certName,
                        certificateType: $certificateType,
                        providerName: $providerName,
                        description: $description,
                        specializations: $specializations,
                        embedding: $embedding
                    })",
                    new
                    {
                        dbId = node.DbId,
                        certCode = node.CertCode,
                        certName = node.CertName,
                        certificateType = node.CertificateType,
                        providerName = node.ProviderName,
                        description = node.Description,
                        specializations = node.Specializations,
                        embedding = node.Embedding
                    });
            });
        }

        public async Task UpdateCertificateNodeAsync(CertificateNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (c:Certificates {dbId: $dbId})
                      SET c.certCode = $certCode,
                          c.certName = $certName,
                          c.certificateType = $certificateType,
                          c.providerName = $providerName,
                          c.description = $description,
                          c.specializations = $specializations,
                          c.embedding = $embedding",
                    new
                    {
                        dbId = node.DbId,
                        certCode = node.CertCode,
                        certName = node.CertName,
                        certificateType = node.CertificateType,
                        providerName = node.ProviderName,
                        description = node.Description,
                        specializations = node.Specializations,
                        embedding = node.Embedding
                    });
            });
        }

        public async Task DeleteCertificateNodeAsync(string dbId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (c:Certificates {dbId: $dbId})
                      DETACH DELETE c",
                    new { dbId });
            });
        }

        #endregion Certificate Node Operations

        #region Gym Node Operations

        public async Task CreateGymNodeAsync(GymNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"CREATE (g:Gym {
                        dbId: $dbId,
                        name: $name,
                        email: $email,
                        businessAddress: $businessAddress,
                        lat: $lat,
                        lon: $lon,
                        openTime: $openTime,
                        closeTime: $closeTime,
                        avgRating: $avgRating,
                        gymOwnerId: $gymOwnerId,
                        gymOwnerName: $gymOwnerName,
                        cheapestCourse: $cheapestCourse,
                        cheapestPrice: $cheapestPrice,
                        courseId: $courseId,
                        embedding: $embedding
                    })",
                    new
                    {
                        dbId = node.DbId,
                        name = node.Name,
                        email = node.Email,
                        businessAddress = node.BusinessAddress,
                        lat = node.Latitude,
                        lon = node.Longitude,
                        openTime = node.OpenTime,
                        closeTime = node.CloseTime,
                        avgRating = node.AverageRating,
                        gymOwnerId = node.GymOwnerId,
                        gymOwnerName = node.GymOwnerName,
                        cheapestCourse = node.CheapestCourse,
                        cheapestPrice = node.CheapestPrice,
                        courseId = node.CourseId,
                        embedding = node.Embedding
                    });
            });
        }

        public async Task UpdateGymNodeAsync(GymNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (g:Gym {dbId: $dbId})
                      SET g.name = $name,
                          g.email = $email,
                          g.businessAddress = $businessAddress,
                          g.lat = $lat,
                          g.lon = $lon,
                          g.openTime = $openTime,
                          g.closeTime = $closeTime,
                          g.avgRating = $avgRating,
                          g.gymOwnerId = $gymOwnerId,
                          g.gymOwnerName = $gymOwnerName,
                          g.cheapestCourse = $cheapestCourse,
                          g.cheapestPrice = $cheapestPrice,
                          g.courseId = $courseId,
                          g.embedding = $embedding",
                    new
                    {
                        dbId = node.DbId,
                        name = node.Name,
                        email = node.Email,
                        businessAddress = node.BusinessAddress,
                        lat = node.Latitude,
                        lon = node.Longitude,
                        openTime = node.OpenTime,
                        closeTime = node.CloseTime,
                        avgRating = node.AverageRating,
                        gymOwnerId = node.GymOwnerId,
                        gymOwnerName = node.GymOwnerName,
                        cheapestCourse = node.CheapestCourse,
                        cheapestPrice = node.CheapestPrice,
                        courseId = node.CourseId,
                        embedding = node.Embedding
                    });
            });
        }

        public async Task DeleteGymNodeAsync(string dbId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (g:Gym {dbId: $dbId})
                      DETACH DELETE g",
                    new { dbId });
            });
        }

        #endregion Gym Node Operations

        #region GymAsset Node Operations

        public async Task CreateGymAssetNodeAsync(GymAssetNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"CREATE (a:GymAsset {
                        dbId: $dbId,
                        name: $name,
                        description: $description,
                        assetType: $assetType,
                        embedding: $embedding
                    })",
                    new
                    {
                        dbId = node.DbId,
                        name = node.Name,
                        description = node.Description,
                        assetType = node.AssetType,
                        embedding = node.Embedding
                    });
            });
        }

        public async Task UpdateGymAssetNodeAsync(GymAssetNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (a:GymAsset {dbId: $dbId})
                      SET a.name = $name,
                          a.description = $description,
                          a.assetType = $assetType,
                          a.embedding = $embedding",
                    new
                    {
                        dbId = node.DbId,
                        name = node.Name,
                        description = node.Description,
                        assetType = node.AssetType,
                        embedding = node.Embedding
                    });
            });
        }

        public async Task DeleteGymAssetNodeAsync(string dbId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (a:GymAsset {dbId: $dbId})
                      DETACH DELETE a",
                    new { dbId });
            });
        }

        #endregion GymAsset Node Operations

        #region Muscles Node Operations

        public async Task CreateMuscleNodeAsync(MusclesNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"CREATE (m:Muscles {name: $name})",
                    new { name = node.Name });
            });
        }

        public async Task UpdateMuscleNodeAsync(string oldName, MusclesNode node, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (m:Muscles {name: $oldName})
                      SET m.name = $newName",
                    new { oldName, newName = node.Name });
            });
        }

        public async Task DeleteMuscleNodeAsync(string name, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (m:Muscles {name: $name})
                      DETACH DELETE m",
                    new { name });
            });
        }

        #endregion Muscles Node Operations

        #region HasCertificate Relationship Operations

        public async Task CreateHasCertificateRelationshipAsync(HasCertificateRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (f:FreelancePT {dbId: $freelancePTId})
                      MATCH (c:Certificates {dbId: $certificateId})
                      CREATE (f)-[r:HAS_CERTIFICATE {
                          certificateStatus: $certificateStatus,
                          certUrl: $certUrl,
                          providedDate: date($providedDate),
                          expirationDate: date($expirationDate)
                      }]->(c)",
                    new
                    {
                        freelancePTId = relationship.FreelancePTId,
                        certificateId = relationship.CertificateId,
                        certificateStatus = relationship.CertificateStatus,
                        certUrl = relationship.CertUrl,
                        providedDate = relationship.ProvidedDate.ToString("yyyy-MM-dd"),
                        expirationDate = relationship.ExpirationDate.ToString("yyyy-MM-dd")
                    });
            });
        }

        public async Task UpdateHasCertificateRelationshipAsync(HasCertificateRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (f:FreelancePT {dbId: $freelancePTId})-[r:HAS_CERTIFICATE]->(c:Certificates {dbId: $certificateId})
                      SET r.certificateStatus = $certificateStatus,
                          r.certUrl = $certUrl,
                          r.providedDate = date($providedDate),
                          r.expirationDate = date($expirationDate)",
                    new
                    {
                        freelancePTId = relationship.FreelancePTId,
                        certificateId = relationship.CertificateId,
                        certificateStatus = relationship.CertificateStatus,
                        certUrl = relationship.CertUrl,
                        providedDate = relationship.ProvidedDate.ToString("yyyy-MM-dd"),
                        expirationDate = relationship.ExpirationDate.ToString("yyyy-MM-dd")
                    });
            });
        }

        public async Task DeleteHasCertificateRelationshipAsync(string freelancePTId, string certificateId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (f:FreelancePT {dbId: $freelancePTId})-[r:HAS_CERTIFICATE]->(c:Certificates {dbId: $certificateId})
                      DELETE r",
                    new { freelancePTId, certificateId });
            });
        }

        #endregion HasCertificate Relationship Operations

        #region Owns Relationship Operations

        public async Task CreateOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (o {dbId: $gymOwnerId})
                      MATCH (g:Gym {dbId: $gymId})
                      CREATE (o)-[r:OWNS {
                          ownershipStartDate: date($ownershipStartDate)
                      }]->(g)",
                    new
                    {
                        gymOwnerId = relationship.GymOwnerId,
                        gymId = relationship.GymId,
                        ownershipStartDate = relationship.OwnershipStartDate.ToString("yyyy-MM-dd")
                    });
            });
        }

        public async Task UpdateOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (o {dbId: $gymOwnerId})-[r:OWNS]->(g:Gym {dbId: $gymId})
                      SET r.ownershipStartDate = date($ownershipStartDate)",
                    new
                    {
                        gymOwnerId = relationship.GymOwnerId,
                        gymId = relationship.GymId,
                        ownershipStartDate = relationship.OwnershipStartDate.ToString("yyyy-MM-dd")
                    });
            });
        }

        public async Task DeleteOwnsRelationshipAsync(string gymOwnerId, string gymId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (o {dbId: $gymOwnerId})-[r:OWNS]->(g:Gym {dbId: $gymId})
                      DELETE r",
                    new { gymOwnerId, gymId });
            });
        }

        #endregion Owns Relationship Operations

        #region Targets Relationship Operations

        public async Task CreateTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (s {dbId: $sourceId})
                      MATCH (m:Muscles {name: $muscleId})
                      CREATE (s)-[r:TARGETS {
                          targetIntensity: $targetIntensity
                      }]->(m)",
                    new
                    {
                        sourceId = relationship.SourceId,
                        muscleId = relationship.MuscleId,
                        targetIntensity = relationship.TargetIntensity
                    });
            });
        }

        public async Task UpdateTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (s {dbId: $sourceId})-[r:TARGETS]->(m:Muscles {name: $muscleId})
                      SET r.targetIntensity = $targetIntensity",
                    new
                    {
                        sourceId = relationship.SourceId,
                        muscleId = relationship.MuscleId,
                        targetIntensity = relationship.TargetIntensity
                    });
            });
        }

        public async Task DeleteTargetsRelationshipAsync(string sourceId, string muscleId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (s {dbId: $sourceId})-[r:TARGETS]->(m:Muscles {name: $muscleId})
                      DELETE r",
                    new { sourceId, muscleId });
            });
        }

        #endregion Targets Relationship Operations
    }
}