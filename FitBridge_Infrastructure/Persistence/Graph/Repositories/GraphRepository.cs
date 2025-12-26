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

        public async Task<FreelancePTNode?> GetFreelancePTByIdAsync(string dbId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (f:FreelancePT {dbId: $dbId})
                      RETURN f",
                    new { dbId });

                return await cursor.SingleAsync(record =>
                    MapToFreelancePTNode(record["f"].As<INode>()));
            });

            return result;
        }

        public async Task<IEnumerable<FreelancePTNode>> GetAllFreelancePTsAsync(CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync("MATCH (f:FreelancePT) RETURN f");
                var records = await cursor.ToListAsync();
                return records.Select(record => MapToFreelancePTNode(record["f"].As<INode>()));
            });

            return result;
        }

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
                        dob: $dob,
                        businessAddress: $businessAddress,
                        lat: $lat,
                        lon: $lon,
                        courseDescription: $courseDescription,
                        cheapestCourse: $cheapestCourse,
                        cheapestPrice: $cheapestPrice,
                        freelancePtCourseId: $freelancePtCourseId
                    })",
                    new
                    {
                        dbId = node.DbId,
                        fullName = node.FullName,
                        email = node.Email,
                        phoneNumber = node.PhoneNumber,
                        isMale = node.IsMale,
                        dob = new ZonedDateTime(node.DateOfBirth),
                        businessAddress = node.BusinessAddress,
                        lat = node.Latitude,
                        lon = node.Longitude,
                        courseDescription = node.CourseDescription,
                        cheapestCourse = node.CheapestCourse,
                        cheapestPrice = node.CheapestPrice,
                        freelancePtCourseId = node.FreelancePtCourseId
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
                          f.dob = $dob,
                          f.businessAddress = $businessAddress,
                          f.lat = $lat,
                          f.lon = $lon,
                          f.courseDescription = $courseDescription,
                          f.cheapestCourse = $cheapestCourse,
                          f.cheapestPrice = $cheapestPrice,
                          f.freelancePtCourseId = $freelancePtCourseId",
                    new
                    {
                        dbId = node.DbId,
                        fullName = node.FullName,
                        email = node.Email,
                        phoneNumber = node.PhoneNumber,
                        isMale = node.IsMale,
                        dob = new ZonedDateTime(node.DateOfBirth),
                        businessAddress = node.BusinessAddress,
                        lat = node.Latitude,
                        lon = node.Longitude,
                        courseDescription = node.CourseDescription,
                        cheapestCourse = node.CheapestCourse,
                        cheapestPrice = node.CheapestPrice,
                        freelancePtCourseId = node.FreelancePtCourseId
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

        public async Task<CertificateNode?> GetCertificateByIdAsync(string dbId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (c:Certificates {dbId: $dbId})
                      RETURN c",
                    new { dbId });

                return await cursor.SingleAsync(record =>
                    MapToCertificateNode(record["c"].As<INode>()));
            });

            return result;
        }

        public async Task<IEnumerable<CertificateNode>> GetAllCertificatesAsync(CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync("MATCH (c:Certificates) RETURN c");
                var records = await cursor.ToListAsync();
                return records.Select(record => MapToCertificateNode(record["c"].As<INode>()));
            });

            return result;
        }

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

        public async Task<GymNode?> GetGymByIdAsync(string dbId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (g:Gym {dbId: $dbId})
                      RETURN g",
                    new { dbId });

                return await cursor.SingleAsync(record =>
                    MapToGymNode(record["g"].As<INode>()));
            });

            return result;
        }

        public async Task<IEnumerable<GymNode>> GetAllGymsAsync(CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync("MATCH (g:Gym) RETURN g");
                var records = await cursor.ToListAsync();
                return records.Select(record => MapToGymNode(record["g"].As<INode>()));
            });

            return result;
        }

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
                        courseId: $courseId
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
                        courseId = node.CourseId
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
                          g.courseId = $courseId",
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
                        courseId = node.CourseId
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

        public async Task<GymAssetNode?> GetGymAssetByIdAsync(string dbId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (a:GymAsset {dbId: $dbId})
                      RETURN a",
                    new { dbId });

                return await cursor.SingleAsync(record =>
                    MapToGymAssetNode(record["a"].As<INode>()));
            });

            return result;
        }

        public async Task<IEnumerable<GymAssetNode>> GetAllGymAssetsAsync(CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync("MATCH (a:GymAsset) RETURN a");
                var records = await cursor.ToListAsync();
                return records.Select(record => MapToGymAssetNode(record["a"].As<INode>()));
            });

            return result;
        }

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

        public async Task<MusclesNode?> GetMuscleByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (m:Muscles {name: $name})
                      RETURN m",
                    new { name });

                return await cursor.SingleAsync(record =>
                    MapToMusclesNode(record["m"].As<INode>()));
            });

            return result;
        }

        public async Task<IEnumerable<MusclesNode>> GetAllMusclesAsync(CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync("MATCH (m:Muscles) RETURN m");
                var records = await cursor.ToListAsync();
                return records.Select(record => MapToMusclesNode(record["m"].As<INode>()));
            });

            return result;
        }

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
                          providedDate: $providedDate,
                          expirationDate: $expirationDate
                      }]->(c)",
                    new
                    {
                        freelancePTId = relationship.FreelancePTId,
                        certificateId = relationship.CertificateId,
                        certificateStatus = relationship.CertificateStatus,
                        certUrl = relationship.CertUrl,
                        providedDate = new LocalDate(relationship.ProvidedDate),
                        expirationDate = new LocalDate(relationship.ExpirationDate)
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
                          r.providedDate = $providedDate,
                          r.expirationDate = $expirationDate",
                    new
                    {
                        freelancePTId = relationship.FreelancePTId,
                        certificateId = relationship.CertificateId,
                        certificateStatus = relationship.CertificateStatus,
                        certUrl = relationship.CertUrl,
                        providedDate = new LocalDate(relationship.ProvidedDate),
                        expirationDate = new LocalDate(relationship.ExpirationDate)
                    });
            });
        }

        public async Task DeleteHasCertificateRelationshipAsync(HasCertificateRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (f:FreelancePT {dbId: $freelancePTId})-[r:HAS_CERTIFICATE]->(c:Certificates {dbId: $certificateId})
                      DELETE r",
                    new { freelancePTId = relationship.FreelancePTId, certificateId = relationship.CertificateId });
            });
        }

        public async Task<IEnumerable<CertificateNode>> GetCertificatesForFreelancePTAsync(string freelancePTId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (f:FreelancePT {dbId: $freelancePTId})-[:HAS_CERTIFICATE]->(c:Certificates)
                      RETURN c",
                    new { freelancePTId });

                var records = await cursor.ToListAsync();
                return records.Select(record => MapToCertificateNode(record["c"].As<INode>()));
            });

            return result;
        }

        #endregion HasCertificate Relationship Operations

        #region Owns Relationship Operations

        public async Task CreateOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (o:Gym {dbId: $gymOwnerId})
                      MATCH (g:GymAsset {dbId: $gymId})
                      CREATE (o)-[r:OWNS]->(g)",
                    new
                    {
                        gymOwnerId = relationship.GymOwnerId,
                        gymId = relationship.GymAssetId
                    });
            });
        }

        public async Task UpdateOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default)
        {
            // No properties to update for OWNS relationship currently
            await Task.CompletedTask;
        }

        public async Task DeleteOwnsRelationshipAsync(OwnsRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (o:Gym {dbId: $gymOwnerId})-[r:OWNS]->(g:GymAsset {dbId: $gymId})
                      DELETE r",
                    new { gymOwnerId = relationship.GymOwnerId, gymId = relationship.GymAssetId });
            });
        }

        public async Task<IEnumerable<GymNode>> GetGymsOwnedByAsync(string gymOwnerId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (o:Gym {dbId: $gymOwnerId})-[:OWNS]->(g:GymAsset)
                      RETURN g",
                    new { gymOwnerId });

                var records = await cursor.ToListAsync();
                return records.Select(record => MapToGymNode(record["g"].As<INode>()));
            });

            return result;
        }

        #endregion Owns Relationship Operations

        #region Targets Relationship Operations

        public async Task CreateTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (s: GymAsset {dbId: $sourceId})
                      UNWIND $muscleNames AS mName
                      MATCH (m:Muscles {name: mName})
                      MERGE (s)-[r:TARGETS]->(m)",
                    new
                    {
                        sourceId = relationship.GymAssetId,
                        muscleName = relationship.MuscleNames
                    });
            });
        }

        public async Task UpdateTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default)
        {
            // No properties to update for TARGETS relationship currently
            await Task.CompletedTask;
        }

        public async Task DeleteTargetsRelationshipAsync(TargetsRelationship relationship, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (s {dbId: $sourceId})
                    UNWIND $muscleNames AS mName
                    MATCH (s)-[r:TARGETS]->(m:Muscles {name: mName})
                    DELETE r",
                    new { sourceId = relationship.GymAssetId, muscleNames = relationship.MuscleNames });
            });
        }

        public async Task<IEnumerable<MusclesNode>> GetTargetedMusclesAsync(string sourceId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (s {dbId: $sourceId})-[:TARGETS]->(m:Muscles)
                      RETURN m",
                    new { sourceId });

                var records = await cursor.ToListAsync();
                return records.Select(record => MapToMusclesNode(record["m"].As<INode>()));
            });

            return result;
        }

        #endregion Targets Relationship Operations

        #region Advanced Query Operations

        public async Task<IEnumerable<FreelancePTNode>> GetNearbyFreelancePTsAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (f:FreelancePT)
                      WITH f, point.distance(
                          point({latitude: f.lat, longitude: f.lon}),
                          point({latitude: $latitude, longitude: $longitude})
                      ) AS distance
                      WHERE distance <= $radiusMeters
                      RETURN f
                      ORDER BY distance",
                    new { latitude, longitude, radiusMeters = radiusKm * 1000 });

                var records = await cursor.ToListAsync();
                return records.Select(record => MapToFreelancePTNode(record["f"].As<INode>()));
            });

            return result;
        }

        public async Task<IEnumerable<GymNode>> GetNearbyGymsAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (g:Gym)
                      WITH g, point.distance(
                          point({latitude: g.lat, longitude: g.lon}),
                          point({latitude: $latitude, longitude: $longitude})
                      ) AS distance
                      WHERE distance <= $radiusMeters
                      RETURN g
                      ORDER BY distance",
                    new { latitude, longitude, radiusMeters = radiusKm * 1000 });

                var records = await cursor.ToListAsync();
                return records.Select(record => MapToGymNode(record["g"].As<INode>()));
            });

            return result;
        }

        public async Task<IEnumerable<FreelancePTNode>> GetFreelancePTsBySpecializationAsync(string specialization, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (f:FreelancePT)-[:HAS_CERTIFICATE]->(c:Certificates)
                      WHERE $specialization IN c.specializations
                      RETURN DISTINCT f",
                    new { specialization });

                var records = await cursor.ToListAsync();
                return records.Select(record => MapToFreelancePTNode(record["f"].As<INode>()));
            });

            return result;
        }

        public async Task<IEnumerable<GymNode>> GetGymsByRatingAsync(double minRating, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (g:Gym)
                      WHERE g.avgRating >= $minRating
                      RETURN g
                      ORDER BY g.avgRating DESC",
                    new { minRating });

                var records = await cursor.ToListAsync();
                return records.Select(record => MapToGymNode(record["g"].As<INode>()));
            });

            return result;
        }

        public async Task<IEnumerable<GymAssetNode>> GetAssetsForGymAsync(string gymId, CancellationToken cancellationToken = default)
        {
            await using var session = _driver.AsyncSession();
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (g:Gym {dbId: $gymId})-[:HAS_ASSET]->(a:GymAsset)
                      RETURN a",
                    new { gymId });

                var records = await cursor.ToListAsync();
                return records.Select(record => MapToGymAssetNode(record["a"].As<INode>()));
            });

            return result;
        }

        #endregion Advanced Query Operations

        #region Mapping Methods

        private static FreelancePTNode MapToFreelancePTNode(INode node)
        {
            return new FreelancePTNode
            {
                DbId = node.Properties["dbId"].As<string>(),
                FullName = node.Properties["fullName"].As<string>(),
                Email = node.Properties["email"].As<string>(),
                PhoneNumber = node.Properties["phoneNumber"].As<string>(),
                IsMale = node.Properties["isMale"].As<bool>(),
                DateOfBirth = node.Properties["dob"].As<ZonedDateTime>().ToDateTimeOffset().UtcDateTime,
                BusinessAddress = node.Properties["businessAddress"].As<string>(),
                Latitude = node.Properties["lat"].As<double>(),
                Longitude = node.Properties["lon"].As<double>(),
                CourseDescription = node.Properties["courseDescription"].As<string>(),
                CheapestCourse = node.Properties["cheapestCourse"].As<string>(),
                CheapestPrice = node.Properties["cheapestPrice"].As<decimal>(),
                FreelancePtCourseId = node.Properties["freelancePtCourseId"].As<string>()
            };
        }

        private static CertificateNode MapToCertificateNode(INode node)
        {
            return new CertificateNode
            {
                DbId = node.Properties["dbId"].As<string>(),
                CertCode = node.Properties["certCode"].As<string>(),
                CertName = node.Properties["certName"].As<string>(),
                CertificateType = node.Properties["certificateType"].As<string>(),
                ProviderName = node.Properties["providerName"].As<string>(),
                Description = node.Properties["description"].As<string>(),
                Specializations = node.Properties["specializations"].As<List<string>>(),
                Embedding = node.Properties.ContainsKey("embedding") ? node.Properties["embedding"].As<List<float>>() : null
            };
        }

        private static GymNode MapToGymNode(INode node)
        {
            return new GymNode
            {
                DbId = node.Properties["dbId"].As<string>(),
                Name = node.Properties["name"].As<string>(),
                Email = node.Properties["email"].As<string>(),
                BusinessAddress = node.Properties["businessAddress"].As<string>(),
                Latitude = node.Properties["lat"].As<double>(),
                Longitude = node.Properties["lon"].As<double>(),
                OpenTime = node.Properties["openTime"].As<string>(),
                CloseTime = node.Properties["closeTime"].As<string>(),
                AverageRating = node.Properties["avgRating"].As<double>(),
                GymOwnerId = node.Properties["gymOwnerId"].As<string>(),
                GymOwnerName = node.Properties["gymOwnerName"].As<string>(),
                CheapestCourse = node.Properties["cheapestCourse"].As<string>(),
                CheapestPrice = node.Properties["cheapestPrice"].As<decimal>(),
                CourseId = node.Properties["courseId"].As<string>()
            };
        }

        private static GymAssetNode MapToGymAssetNode(INode node)
        {
            return new GymAssetNode
            {
                DbId = node.Properties["dbId"].As<string>(),
                Name = node.Properties["name"].As<string>(),
                Description = node.Properties["description"].As<string>(),
                AssetType = node.Properties["assetType"].As<string>(),
                Embedding = node.Properties.ContainsKey("embedding") ? node.Properties["embedding"].As<List<float>>() : null
            };
        }

        private static MusclesNode MapToMusclesNode(INode node)
        {
            return new MusclesNode
            {
                Name = node.Properties["name"].As<string>()
            };
        }

        #endregion Mapping Methods
    }
}