using AnimeGraphApi.Models;

namespace AnimeGraphApi.Services;

public interface IGraphService
{
    Task<PagedResult<RelationNodeDto>> GetRelationsAsync(string name, int skip, int limit);
}
