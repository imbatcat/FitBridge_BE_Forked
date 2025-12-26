using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Graph.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FitBridge_API.Controllers
{
    [Route("api/graph/gym-assets")]
    [ApiController]
    public class GraphGymAssetController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphGymAssetController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GymAssetNode node)
        {
            await _graphService.CreateNode(node);
            return Ok(new { message = "GymAsset node created successfully with embedding" });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] GymAssetNode node)
        {
            await _graphService.UpdateNode(node);
            return Ok(new { message = "GymAsset node updated successfully with embedding" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] GymAssetNode node)
        {
            await _graphService.DeleteNode(node);
            return Ok(new { message = "GymAsset node deleted successfully" });
        }
    }
}
