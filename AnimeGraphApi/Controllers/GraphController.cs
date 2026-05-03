using AnimeGraphApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnimeGraphApi.Controllers;

[ApiController]
[Route("relations")]
[Produces("application/json")]
public class GraphController : ControllerBase
{
    private readonly IGraphService _graphService;

    public GraphController(IGraphService graphService)
    {
        _graphService = graphService;
    }

    /// <summary>Lista todos os nós relacionados a um nó (distância máxima 1), pesquisando pelo nome.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRelations(
        [FromQuery] string name,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "O parâmetro 'name' é obrigatório." });

        var result = await _graphService.GetRelationsAsync(name, skip, Math.Min(limit, 200));
        return Ok(result);
    }
}
