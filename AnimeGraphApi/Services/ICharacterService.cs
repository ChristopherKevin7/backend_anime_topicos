using AnimeGraphApi.Models;

namespace AnimeGraphApi.Services;

public interface ICharacterService
{
    Task<List<string>> SearchNameAsync(string query, int limit);
    Task<PagedResult<VoiceActorDto>> GetVoiceActorsAsync(string name, int skip, int limit);
    Task<PagedResult<RelatedCharacterDto>> GetRelatedByVoiceActorAsync(string name, int skip, int limit);
    Task<PathDto?> GetPathBetweenCharactersAsync(string from, string to);
}
