using AnimeGraphApi.Config;
using AnimeGraphApi.Models;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace AnimeGraphApi.Repositories;

public class GraphRepository : IGraphRepository
{
    private readonly IDriver _driver;
    private readonly string _database;

    public GraphRepository(IDriver driver, IOptions<Neo4jSettings> settings)
    {
        _driver = driver;
        _database = settings.Value.Database;
    }

    public async Task<PagedResult<RelationNodeDto>> GetRelationsAsync(string name, int skip, int limit)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));

        var countQuery = @"
            MATCH (n)-[r]-(neighbor)
            WHERE toLower(coalesce(n.name, n.title, '')) = toLower($name)
            RETURN count(neighbor) AS total";

        var dataQuery = @"
            MATCH (n)-[r]-(neighbor)
            WHERE toLower(coalesce(n.name, n.title, '')) = toLower($name)
            RETURN coalesce(neighbor.title, neighbor.name, '') AS neighborName,
                   labels(neighbor)[0] AS neighborType,
                   type(r) AS relType,
                   r.role AS relRole
            SKIP toInteger($skip) LIMIT toInteger($limit)";

        var (total, items) = await session.ExecuteReadAsync(async tx =>
        {
            var countCursor = await tx.RunAsync(countQuery, new { name });
            var countRecord = await countCursor.SingleAsync();
            var total = countRecord["total"].As<int>();

            var dataCursor = await tx.RunAsync(dataQuery, new { name, skip, limit });
            var records = await dataCursor.ToListAsync();
            var items = records.Select(r => new RelationNodeDto
            {
                Name = r["neighborName"].As<string>(),
                Type = r["neighborType"].As<string?>() ?? string.Empty,
                RelationshipType = r["relType"].As<string>(),
                RelationshipRole = r["relRole"].As<string?>()
            }).ToList();

            return (total, items);
        });

        return new PagedResult<RelationNodeDto> { Items = items, Total = total, Skip = skip, Limit = limit };
    }
}
