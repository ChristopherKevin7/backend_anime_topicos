using AnimeGraphApi.Models;
using AnimeGraphApi.Repositories;

namespace AnimeGraphApi.Services;

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _repository;

    public CharacterService(ICharacterRepository repository)
    {
        _repository = repository;
    }

    public Task<List<string>> SearchNameAsync(string query, int limit) =>
        _repository.SearchNameAsync(query, limit);

    public Task<PagedResult<VoiceActorDto>> GetVoiceActorsAsync(string name, int skip, int limit) =>
        _repository.GetVoiceActorsAsync(name, skip, limit);

    public Task<PagedResult<RelatedCharacterDto>> GetRelatedByVoiceActorAsync(string name, int skip, int limit) =>
        _repository.GetRelatedByVoiceActorAsync(name, skip, limit);

    public Task<PathDto?> GetPathBetweenCharactersAsync(string from, string to) =>
        _repository.GetPathBetweenCharactersAsync(from, to);
}
