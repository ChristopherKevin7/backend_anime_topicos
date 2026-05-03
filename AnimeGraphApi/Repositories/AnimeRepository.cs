using AnimeGraphApi.Config;
using AnimeGraphApi.Models;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace AnimeGraphApi.Repositories;

public class AnimeRepository : IAnimeRepository
{
    private readonly IDriver _driver;
    private readonly string _database;

    public AnimeRepository(IDriver driver, IOptions<Neo4jSettings> settings)
    {
        _driver = driver;
        _database = settings.Value.Database;
    }

    public async Task<List<string>> SearchTitleAsync(string query, int limit)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        var cypher = @"
            MATCH (a:ANIME)
            WHERE toLower(a.title) CONTAINS toLower($searchTerm)
            RETURN a.title AS title
            ORDER BY a.title
            LIMIT toInteger($maxResults)";

        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(cypher, new { searchTerm = query, maxResults = limit });
            var records = await cursor.ToListAsync();
            return records.Select(r => r["title"].As<string>()).ToList();
        });
    }

    public async Task<AnimeDto?> GetAnimeByTitleAsync(string title)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        // Use WITH aggregation chains to avoid Cartesian product from multiple OPTIONAL MATCHes.
        // CASE WHEN ensures null nodes from OPTIONAL MATCH are not added to the collected list.
        var mainQuery = @"
            MATCH (a:ANIME)
            WHERE toLower(a.title) CONTAINS toLower($title)
            WITH a, CASE WHEN toLower(a.title) = toLower($title) THEN 0 ELSE 1 END AS rank
            ORDER BY rank, a.title
            LIMIT 1
            WITH a
            OPTIONAL MATCH (a)-[:HAS_GENRE]->(g:GENRE)
            WITH a, collect(DISTINCT g.name) AS genres
            OPTIONAL MATCH (a)-[:PRODUCED_BY]->(p)
            WITH a, genres,
                 collect(CASE WHEN p IS NOT NULL THEN {name: p.name, ptype: labels(p)[0]} ELSE null END) AS producers
            OPTIONAL MATCH (a)-[sr:HAS_STAFF]->(s:STAFF)
            RETURN a.title AS title,
                   a.score AS score,
                   a.episodes AS episodes,
                   a.anime_type AS animeType,
                   genres,
                   producers,
                   collect(CASE WHEN s IS NOT NULL THEN {name: s.name, role: sr.role} ELSE null END) AS staff";

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(mainQuery, new { title });
            return await cursor.ToListAsync();
        });

        if (result.Count == 0) return null;

        var record = result[0];

        var producerNodes = record["producers"].As<List<IDictionary<string, object>>>();
        var studios = new List<string>();
        var producers = new List<string>();
        var licensors = new List<string>();

        foreach (var p in producerNodes)
        {
            var name = p["name"]?.ToString();
            var type = p["ptype"]?.ToString();
            if (string.IsNullOrEmpty(name)) continue;

            if (type == "STUDIO") studios.Add(name);
            else if (type == "PRODUCER") producers.Add(name);
            else if (type == "LICENSOR") licensors.Add(name);
        }

        var staffRaw = record["staff"].As<List<IDictionary<string, object>>>();
        var staff = staffRaw
            .Where(s => !string.IsNullOrEmpty(s["name"]?.ToString()))
            .Select(s => new StaffEntryDto
            {
                Name = s["name"]!.ToString()!,
                Role = s["role"]?.ToString()
            })
            .ToList();

        return new AnimeDto
        {
            Title = record["title"].As<string>(),
            Score = record["score"].As<double?>(),
            Episodes = record["episodes"].As<double?>() is double ep ? (int?)Convert.ToInt32(ep) : null,
            AnimeType = record["animeType"].As<string?>(),
            Genres = record["genres"].As<List<object>>().Select(x => x.ToString()!).Where(x => !string.IsNullOrEmpty(x)).ToList(),
            Studios = studios,
            Producers = producers,
            Licensors = licensors,
            Staff = staff
        };
    }

    public async Task<PagedResult<CharacterDto>> GetCharactersAsync(string title, int skip, int limit)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        var countQuery = @"
            MATCH (a:ANIME)
            WHERE toLower(a.title) CONTAINS toLower($title)
            WITH a, CASE WHEN toLower(a.title) = toLower($title) THEN 0 ELSE 1 END AS rank
            ORDER BY rank, a.title
            LIMIT 1
            OPTIONAL MATCH (a)-[r:FEATURES_CHARACTER]->(c:CHARACTER)
            WHERE NOT c.name STARTS WITH 'Unknown'
            RETURN a.title AS matchedTitle, count(c) AS total";

        var dataQuery = @"
            MATCH (a:ANIME)
            WHERE toLower(a.title) CONTAINS toLower($title)
            WITH a, CASE WHEN toLower(a.title) = toLower($title) THEN 0 ELSE 1 END AS rank
            ORDER BY rank, a.title
            LIMIT 1
            MATCH (a)-[r:FEATURES_CHARACTER]->(c:CHARACTER)
            WHERE NOT c.name STARTS WITH 'Unknown'
            RETURN c.name AS name, r.role AS role
            ORDER BY r.role, c.name
            SKIP toInteger($skip) LIMIT toInteger($limit)";

        var (total, matchedTitle, items) = await session.ExecuteReadAsync(async tx =>
        {
            var countCursor = await tx.RunAsync(countQuery, new { title });
            var countRecord = await countCursor.SingleAsync();
            var total = countRecord["total"].As<int>();
            var matchedTitle = countRecord["matchedTitle"].As<string?>();

            var dataCursor = await tx.RunAsync(dataQuery, new { title, skip, limit });
            var records = await dataCursor.ToListAsync();
            var items = records.Select(r => new CharacterDto
            {
                Name = r["name"].As<string>(),
                Role = r["role"].As<string?>()
            }).ToList();

            return (total, matchedTitle, items);
        });

        return new PagedResult<CharacterDto> { Items = items, Total = total, Skip = skip, Limit = limit, MatchedTitle = matchedTitle };
    }

    public async Task<PagedResult<RelatedAnimeDto>> GetRelatedByStudioAsync(string title, int skip, int limit)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        var countQuery = @"
            MATCH (a:ANIME)-[:PRODUCED_BY]->(s:STUDIO)<-[:PRODUCED_BY]-(other:ANIME)
            WHERE toLower(a.title) = toLower($title) AND a <> other
            RETURN count(DISTINCT other) AS total";

        var dataQuery = @"
            MATCH (a:ANIME)-[:PRODUCED_BY]->(s:STUDIO)<-[:PRODUCED_BY]-(other:ANIME)
            WHERE toLower(a.title) = toLower($title) AND a <> other
            RETURN DISTINCT other.title AS title, other.score AS score, other.anime_type AS animeType, s.name AS sharedEntity
            ORDER BY other.score DESC
            SKIP toInteger($skip) LIMIT toInteger($limit)";

        var (total, items) = await session.ExecuteReadAsync(async tx =>
        {
            var countCursor = await tx.RunAsync(countQuery, new { title });
            var countRecord = await countCursor.SingleAsync();
            var total = countRecord["total"].As<int>();

            var dataCursor = await tx.RunAsync(dataQuery, new { title, skip, limit });
            var records = await dataCursor.ToListAsync();
            var items = records.Select(r => new RelatedAnimeDto
            {
                Title = r["title"].As<string>(),
                Score = r["score"].As<double?>(),
                AnimeType = r["animeType"].As<string?>(),
                SharedEntity = r["sharedEntity"].As<string?>()
            }).ToList();

            return (total, items);
        });

        return new PagedResult<RelatedAnimeDto> { Items = items, Total = total, Skip = skip, Limit = limit };
    }

    public async Task<PagedResult<RelatedAnimeDto>> GetRelatedByStaffAsync(string title, int skip, int limit)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        var countQuery = @"
            MATCH (a:ANIME)-[:HAS_STAFF]->(s:STAFF)<-[:HAS_STAFF]-(other:ANIME)
            WHERE toLower(a.title) = toLower($title) AND a <> other
            RETURN count(DISTINCT other) AS total";

        var dataQuery = @"
            MATCH (a:ANIME)-[r1:HAS_STAFF]->(s:STAFF)<-[:HAS_STAFF]-(other:ANIME)
            WHERE toLower(a.title) = toLower($title) AND a <> other
            RETURN DISTINCT other.title AS title, other.score AS score, other.anime_type AS animeType, s.name AS sharedEntity, r1.role AS sharedEntityRole
            ORDER BY other.score DESC
            SKIP toInteger($skip) LIMIT toInteger($limit)";

        var (total, items) = await session.ExecuteReadAsync(async tx =>
        {
            var countCursor = await tx.RunAsync(countQuery, new { title });
            var countRecord = await countCursor.SingleAsync();
            var total = countRecord["total"].As<int>();

            var dataCursor = await tx.RunAsync(dataQuery, new { title, skip, limit });
            var records = await dataCursor.ToListAsync();
            var items = records.Select(r => new RelatedAnimeDto
            {
                Title = r["title"].As<string>(),
                Score = r["score"].As<double?>(),
                AnimeType = r["animeType"].As<string?>(),
                SharedEntity = r["sharedEntity"].As<string?>(),
                SharedEntityRole = r["sharedEntityRole"].As<string?>()
            }).ToList();

            return (total, items);
        });

        return new PagedResult<RelatedAnimeDto> { Items = items, Total = total, Skip = skip, Limit = limit };
    }
}
