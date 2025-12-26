using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Graph.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FitBridge_API.Controllers
{
    [Route("api/graph/certificates")]
    [ApiController]
    public class GraphCertificateController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphCertificateController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CertificateNode node)
        {
            await _graphService.CreateNode(node);
            return Ok(new { message = "Certificate node created successfully with embedding" });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CertificateNode node)
        {
            await _graphService.UpdateNode(node);
            return Ok(new { message = "Certificate node updated successfully with embedding" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] CertificateNode node)
        {
            await _graphService.DeleteNode(node);
            return Ok(new { message = "Certificate node deleted successfully" });
        }
    }
}
