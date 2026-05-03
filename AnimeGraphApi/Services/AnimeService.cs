using AnimeGraphApi.Models;
using AnimeGraphApi.Repositories;

namespace AnimeGraphApi.Services;

public class AnimeService : IAnimeService
{
    private readonly IAnimeRepository _repository;

    public AnimeService(IAnimeRepository repository)
    {
        _repository = repository;
    }

    public Task<AnimeDto?> GetAnimeByTitleAsync(string title) =>
        _repository.GetAnimeByTitleAsync(title);

    public Task<List<string>> SearchTitleAsync(string query, int limit) =>
        _repository.SearchTitleAsync(query, limit);

    public Task<PagedResult<CharacterDto>> GetCharactersAsync(string title, int skip, int limit) =>
        _repository.GetCharactersAsync(title, skip, limit);

    public Task<PagedResult<RelatedAnimeDto>> GetRelatedByStudioAsync(string title, int skip, int limit) =>
        _repository.GetRelatedByStudioAsync(title, skip, limit);

    public Task<PagedResult<RelatedAnimeDto>> GetRelatedByStaffAsync(string title, int skip, int limit) =>
        _repository.GetRelatedByStaffAsync(title, skip, limit);
}
