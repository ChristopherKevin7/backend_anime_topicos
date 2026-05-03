using AnimeGraphApi.Models;
using AnimeGraphApi.Repositories;

namespace AnimeGraphApi.Services;

public class GraphService : IGraphService
{
    private readonly IGraphRepository _repository;

    public GraphService(IGraphRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<RelationNodeDto>> GetRelationsAsync(string name, int skip, int limit) =>
        _repository.GetRelationsAsync(name, skip, limit);
}
