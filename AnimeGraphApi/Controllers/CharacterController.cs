using AnimeGraphApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnimeGraphApi.Controllers;

[ApiController]
[Route("character")]
[Produces("application/json")]
public class CharacterController : ControllerBase
{
    private readonly ICharacterService _characterService;

    public CharacterController(ICharacterService characterService)
    {
        _characterService = characterService;
    }

    /// <summary>Busca personagens por nome parcial (case-insensitive). Útil para descobrir o nome exato.</summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "O parâmetro 'q' é obrigatório." });

        var results = await _characterService.SearchNameAsync(q, Math.Min(limit, 100));
        return Ok(results);
    }

    /// <summary>Lista os dubladores de um personagem por idioma.</summary>
    [HttpGet("{name}/voice-actors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVoiceActors(
        string name,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        var result = await _characterService.GetVoiceActorsAsync(name, skip, Math.Min(limit, 200));
        return Ok(result);
    }

    /// <summary>Lista personagens que compartilham o mesmo dublador.</summary>
    [HttpGet("{name}/related-by-voice-actor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelatedByVoiceActor(
        string name,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        var result = await _characterService.GetRelatedByVoiceActorAsync(name, skip, Math.Min(limit, 200));
        return Ok(result);
    }

    /// <summary>Retorna o caminho mais curto entre dois personagens no grafo.</summary>
    [HttpGet("path")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPath(
        [FromQuery] string from,
        [FromQuery] string to)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            return BadRequest(new { message = "Os parâmetros 'from' e 'to' são obrigatórios." });

        var result = await _characterService.GetPathBetweenCharactersAsync(from, to);
        if (result is null) return NotFound(new { message = $"Nenhum caminho encontrado entre '{from}' e '{to}'." });
        return Ok(result);
    }
}
