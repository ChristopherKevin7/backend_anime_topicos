using AnimeGraphApi.Config;
using AnimeGraphApi.Models;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace AnimeGraphApi.Repositories;

public class CharacterRepository : ICharacterRepository
{
    private readonly IDriver _driver;
    private readonly string _database;

    public CharacterRepository(IDriver driver, IOptions<Neo4jSettings> settings)
    {
        _driver = driver;
        _database = settings.Value.Database;
    }

    public async Task<List<string>> SearchNameAsync(string query, int limit)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        var cypher = @"
            MATCH (c:CHARACTER)
            WHERE toLower(c.name) CONTAINS toLower($searchTerm)
              AND NOT c.name STARTS WITH 'Unknown'
            RETURN c.name AS name
            ORDER BY c.name
            LIMIT toInteger($maxResults)";

        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(cypher, new { searchTerm = query, maxResults = limit });
            var records = await cursor.ToListAsync();
            return records.Select(r => r["name"].As<string>()).ToList();
        });
    }

    public async Task<PagedResult<VoiceActorDto>> GetVoiceActorsAsync(string name, int skip, int limit)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        var countQuery = @"
            MATCH (c:CHARACTER)
            WHERE toLower(c.name) CONTAINS toLower($name)
            WITH c, CASE WHEN toLower(c.name) = toLower($name) THEN 0 ELSE 1 END AS rank
            ORDER BY rank, c.name
            LIMIT 1
            MATCH (c)-[r:VOICED_BY]->(v:VOICE_ACTOR)
            RETURN count(v) AS total";

        var dataQuery = @"
            MATCH (c:CHARACTER)
            WHERE toLower(c.name) CONTAINS toLower($name)
            WITH c, CASE WHEN toLower(c.name) = toLower($name) THEN 0 ELSE 1 END AS rank
            ORDER BY rank, c.name
            LIMIT 1
            MATCH (c)-[r:VOICED_BY]->(v:VOICE_ACTOR)
            RETURN v.name AS name, r.language AS language
            ORDER BY r.language
            SKIP toInteger($skip) LIMIT toInteger($limit)";

        var (total, items) = await session.ExecuteReadAsync(async tx =>
        {
            var countCursor = await tx.RunAsync(countQuery, new { name });
            var countRecord = await countCursor.SingleAsync();
            var total = countRecord["total"].As<int>();

            var dataCursor = await tx.RunAsync(dataQuery, new { name, skip, limit });
            var records = await dataCursor.ToListAsync();
            var items = records.Select(r => new VoiceActorDto
            {
                Name = r["name"].As<string>(),
                Language = r["language"].As<string?>()
            }).ToList();

            return (total, items);
        });

        return new PagedResult<VoiceActorDto> { Items = items, Total = total, Skip = skip, Limit = limit };
    }

    public async Task<PagedResult<RelatedCharacterDto>> GetRelatedByVoiceActorAsync(string name, int skip, int limit)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        var countQuery = @"
            MATCH (c:CHARACTER)
            WHERE toLower(c.name) CONTAINS toLower($name)
            WITH c, CASE WHEN toLower(c.name) = toLower($name) THEN 0 ELSE 1 END AS rank
            ORDER BY rank, c.name
            LIMIT 1
            MATCH (c)-[:VOICED_BY]->(v:VOICE_ACTOR)<-[:VOICED_BY]-(other:CHARACTER)
            WHERE NOT other.name STARTS WITH 'Unknown'
              AND c <> other
            RETURN count(DISTINCT other) AS total";

        var dataQuery = @"
            MATCH (c:CHARACTER)
            WHERE toLower(c.name) CONTAINS toLower($name)
            WITH c, CASE WHEN toLower(c.name) = toLower($name) THEN 0 ELSE 1 END AS rank
            ORDER BY rank, c.name
            LIMIT 1
            MATCH (c)-[:VOICED_BY]->(v:VOICE_ACTOR)<-[:VOICED_BY]-(other:CHARACTER)
            WHERE NOT other.name STARTS WITH 'Unknown'
              AND c <> other
            OPTIONAL MATCH (a:ANIME)-[ar:FEATURES_CHARACTER]->(other)
            RETURN DISTINCT other.name AS characterName, a.title AS animeTitle, ar.role AS role
            SKIP toInteger($skip) LIMIT toInteger($limit)";

        var (total, items) = await session.ExecuteReadAsync(async tx =>
        {
            var countCursor = await tx.RunAsync(countQuery, new { name });
            var countRecord = await countCursor.SingleAsync();
            var total = countRecord["total"].As<int>();

            var dataCursor = await tx.RunAsync(dataQuery, new { name, skip, limit });
            var records = await dataCursor.ToListAsync();
            var items = records.Select(r => new RelatedCharacterDto
            {
                CharacterName = r["characterName"].As<string>(),
                AnimeTitle = r["animeTitle"].As<string?>() ?? string.Empty,
                Role = r["role"].As<string?>()
            }).ToList();

            return (total, items);
        });

        return new PagedResult<RelatedCharacterDto> { Items = items, Total = total, Skip = skip, Limit = limit };
    }

    public async Task<PathDto?> GetPathBetweenCharactersAsync(string from, string to)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        var query = @"
            MATCH (start:CHARACTER)
            WHERE toLower(start.name) CONTAINS toLower($from)
            WITH start, CASE WHEN toLower(start.name) = toLower($from) THEN 0 ELSE 1 END AS rankFrom
            ORDER BY rankFrom, start.name
            LIMIT 1
            MATCH (end:CHARACTER)
            WHERE toLower(end.name) CONTAINS toLower($to)
            WITH start, end, CASE WHEN toLower(end.name) = toLower($to) THEN 0 ELSE 1 END AS rankTo
            ORDER BY rankTo, end.name
            LIMIT 1
            WITH start, end
            MATCH path = shortestPath((start)-[*..10]-(end))
            RETURN [n IN nodes(path) | {name: coalesce(n.title, n.name, ''), type: labels(n)[0]}] AS pathNodes";

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { from, to });
            return await cursor.ToListAsync();
        });

        if (result.Count == 0) return null;

        var pathNodes = result[0]["pathNodes"].As<List<IDictionary<string, object>>>();
        var nodes = pathNodes.Select(n => new PathNodeDto
        {
            Name = n["name"]?.ToString() ?? string.Empty,
            Type = n["type"]?.ToString() ?? string.Empty
        }).ToList();

        return new PathDto { Nodes = nodes };
    }
}
