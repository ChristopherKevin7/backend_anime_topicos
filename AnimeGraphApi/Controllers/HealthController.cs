using AnimeGraphApi.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace AnimeGraphApi.Controllers;

[ApiController]
[Route("health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly IDriver _driver;
    private readonly Neo4jSettings _settings;

    public HealthController(IDriver driver, IOptions<Neo4jSettings> settings)
    {
        _driver = driver;
        _settings = settings.Value;
    }

    /// <summary>Verifica a conectividade com o banco de dados Neo4j.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            await _driver.VerifyConnectivityAsync();
            return Ok(new
            {
                status = "healthy",
                uri = _settings.Uri,
                database = _settings.Database,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "unhealthy",
                uri = _settings.Uri,
                database = _settings.Database,
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Diagnóstico do banco: retorna labels existentes, contagem por tipo e 3 títulos de amostra.
    /// Use este endpoint se os outros retornam vazio para confirmar o schema do banco.
    /// </summary>
    [HttpGet("debug")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Debug()
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_settings.Database));
        try
        {
            var labelCounts = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    "MATCH (n) RETURN labels(n)[0] AS label, count(*) AS c ORDER BY c DESC LIMIT 15");
                var records = await cursor.ToListAsync();
                return records.Select(r => new
                {
                    label = r["label"].As<string?>(),
                    count = r["c"].As<long>()
                }).ToList();
            });

            var sampleTitles = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync("MATCH (a:ANIME) RETURN a.title AS title LIMIT 3");
                var records = await cursor.ToListAsync();
                return records.Select(r => r["title"].As<string?>()).ToList();
            });

            return Ok(new
            {
                database = _settings.Database,
                uri = _settings.Uri,
                labelCounts,
                sampleAnimeTitles = sampleTitles,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { error = ex.Message, database = _settings.Database });
        }
    }
}
