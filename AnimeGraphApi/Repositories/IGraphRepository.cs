using AnimeGraphApi.Models;

namespace AnimeGraphApi.Repositories;

public interface IGraphRepository
{
    Task<PagedResult<RelationNodeDto>> GetRelationsAsync(string name, int skip, int limit);
}
