using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Graph.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FitBridge_API.Controllers
{
    [Route("api/graph/muscles")]
    [ApiController]
    public class GraphMusclesController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphMusclesController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MusclesNode node)
        {
            await _graphService.CreateNode(node);
            return Ok(new { message = "Muscles node created successfully" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] MusclesNode node)
        {
            await _graphService.DeleteNode(node);
            return Ok(new { message = "Muscles node deleted successfully" });
        }
    }
}
