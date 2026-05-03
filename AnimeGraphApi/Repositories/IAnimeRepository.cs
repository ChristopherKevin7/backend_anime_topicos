using AnimeGraphApi.Models;

namespace AnimeGraphApi.Repositories;

public interface IAnimeRepository
{
    Task<AnimeDto?> GetAnimeByTitleAsync(string title);
    Task<List<string>> SearchTitleAsync(string query, int limit);
    Task<PagedResult<CharacterDto>> GetCharactersAsync(string title, int skip, int limit);
    Task<PagedResult<RelatedAnimeDto>> GetRelatedByStudioAsync(string title, int skip, int limit);
    Task<PagedResult<RelatedAnimeDto>> GetRelatedByStaffAsync(string title, int skip, int limit);
}
