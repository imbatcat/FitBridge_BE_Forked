using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Graph.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FitBridge_API.Controllers
{
    [Route("api/graph/freelance-pts")]
    [ApiController]
    public class GraphFreelancePTController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphFreelancePTController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FreelancePTNode node)
        {
            await _graphService.CreateNode(node);
            return Ok(new { message = "FreelancePT node created successfully" });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] FreelancePTNode node)
        {
            await _graphService.UpdateNode(node);
            return Ok(new { message = "FreelancePT node updated successfully" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] FreelancePTNode node)
        {
            await _graphService.DeleteNode(node);
            return Ok(new { message = "FreelancePT node deleted successfully" });
        }
    }
}
