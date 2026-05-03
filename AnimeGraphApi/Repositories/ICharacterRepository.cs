using AnimeGraphApi.Models;

namespace AnimeGraphApi.Repositories;

public interface ICharacterRepository
{
    Task<List<string>> SearchNameAsync(string query, int limit);
    Task<PagedResult<VoiceActorDto>> GetVoiceActorsAsync(string name, int skip, int limit);
    Task<PagedResult<RelatedCharacterDto>> GetRelatedByVoiceActorAsync(string name, int skip, int limit);
    Task<PathDto?> GetPathBetweenCharactersAsync(string from, string to);
}
