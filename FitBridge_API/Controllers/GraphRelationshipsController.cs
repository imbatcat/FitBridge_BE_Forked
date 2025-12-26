using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Graph.Entities.Relationships;
using Microsoft.AspNetCore.Mvc;

namespace FitBridge_API.Controllers
{
    [Route("api/graph/relationships")]
    [ApiController]
    public class GraphRelationshipsController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphRelationshipsController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        // POST: api/graph/relationships/has-certificate
        [HttpPost("has-certificate")]
        public async Task<IActionResult> CreateHasCertificate([FromBody] HasCertificateRelationship relationship)
        {
            await _graphService.CreateRelationship(relationship);
            return Ok(new { message = "HAS_CERTIFICATE relationship created successfully" });
        }

        // PUT: api/graph/relationships/has-certificate
        [HttpPut("has-certificate")]
        public async Task<IActionResult> UpdateHasCertificate([FromBody] HasCertificateRelationship relationship)
        {
            await _graphService.UpdateRelationship(relationship);
            return Ok(new { message = "HAS_CERTIFICATE relationship updated successfully" });
        }

        // DELETE: api/graph/relationships/has-certificate
        [HttpDelete("has-certificate")]
        public async Task<IActionResult> DeleteHasCertificate([FromBody] HasCertificateRelationship relationship)
        {
            await _graphService.DeleteRelationship(relationship);
            return Ok(new { message = "HAS_CERTIFICATE relationship deleted successfully" });
        }

        // POST: api/graph/relationships/owns
        [HttpPost("owns")]
        public async Task<IActionResult> CreateOwns([FromBody] OwnsRelationship relationship)
        {
            await _graphService.CreateRelationship(relationship);
            return Ok(new { message = "OWNS relationship created successfully" });
        }

        // PUT: api/graph/relationships/owns
        [HttpPut("owns")]
        public async Task<IActionResult> UpdateOwns([FromBody] OwnsRelationship relationship)
        {
            await _graphService.UpdateRelationship(relationship);
            return Ok(new { message = "OWNS relationship updated successfully" });
        }

        // DELETE: api/graph/relationships/owns
        [HttpDelete("owns")]
        public async Task<IActionResult> DeleteOwns([FromBody] OwnsRelationship relationship)
        {
            await _graphService.DeleteRelationship(relationship);
            return Ok(new { message = "OWNS relationship deleted successfully" });
        }

        // POST: api/graph/relationships/targets
        [HttpPost("targets")]
        public async Task<IActionResult> CreateTargets([FromBody] TargetsRelationship relationship)
        {
            await _graphService.CreateRelationship(relationship);
            return Ok(new { message = "TARGETS relationship created successfully" });
        }

        // PUT: api/graph/relationships/targets
        [HttpPut("targets")]
        public async Task<IActionResult> UpdateTargets([FromBody] TargetsRelationship relationship)
        {
            await _graphService.UpdateRelationship(relationship);
            return Ok(new { message = "TARGETS relationship updated successfully" });
        }

        // DELETE: api/graph/relationships/targets
        [HttpDelete("targets")]
        public async Task<IActionResult> DeleteTargets([FromBody] TargetsRelationship relationship)
        {
            await _graphService.DeleteRelationship(relationship);
            return Ok(new { message = "TARGETS relationship deleted successfully" });
        }
    }
}
