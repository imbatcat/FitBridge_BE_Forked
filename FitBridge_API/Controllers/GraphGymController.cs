using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Graph.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FitBridge_API.Controllers
{
    [Route("api/graph/gyms")]
    [ApiController]
    public class GraphGymController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphGymController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GymNode node)
        {
            await _graphService.CreateNode(node);
            return Ok(new { message = "Gym node created successfully" });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] GymNode node)
        {
            await _graphService.UpdateNode(node);
            return Ok(new { message = "Gym node updated successfully" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] GymNode node)
        {
            await _graphService.DeleteNode(node);
            return Ok(new { message = "Gym node deleted successfully" });
        }
    }
}
