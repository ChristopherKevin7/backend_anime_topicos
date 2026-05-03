using AnimeGraphApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnimeGraphApi.Controllers;

[ApiController]
[Route("anime")]
[Produces("application/json")]
public class AnimeController : ControllerBase
{
    private readonly IAnimeService _animeService;

    public AnimeController(IAnimeService animeService)
    {
        _animeService = animeService;
    }

    /// <summary>Busca animes por título parcial (case-insensitive). Útil para descobrir o título exato.</summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "O parâmetro 'q' é obrigatório." });

        var results = await _animeService.SearchTitleAsync(q, Math.Min(limit, 100));
        return Ok(results);
    }

    /// <summary>Busca um anime pelo título, retornando dados gerais, gêneros, produtores e staff.</summary>
    [HttpGet("{title}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAnime(string title)
    {
        var result = await _animeService.GetAnimeByTitleAsync(title);
        if (result is null) return NotFound(new { message = $"Anime '{title}' não encontrado." });
        return Ok(result);
    }

    /// <summary>Lista os personagens de um anime com seu papel (Main/Supporting).</summary>
    [HttpGet("{title}/characters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCharacters(
        string title,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        var result = await _animeService.GetCharactersAsync(title, skip, Math.Min(limit, 200));
        return Ok(result);
    }

    /// <summary>Lista outros animes produzidos pelo mesmo estúdio.</summary>
    [HttpGet("{title}/related-by-studio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelatedByStudio(
        string title,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        var result = await _animeService.GetRelatedByStudioAsync(title, skip, Math.Min(limit, 200));
        return Ok(result);
    }

    /// <summary>Lista outros animes que compartilham membros de staff.</summary>
    [HttpGet("{title}/related-by-staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelatedByStaff(
        string title,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        var result = await _animeService.GetRelatedByStaffAsync(title, skip, Math.Min(limit, 200));
        return Ok(result);
    }
}
