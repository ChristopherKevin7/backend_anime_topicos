const BASE_URL = 'http://localhost:5128';

async function request(path, params = {}) {
  const url = new URL(path, BASE_URL);
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== '') url.searchParams.set(k, v);
  });
  const res = await fetch(url);
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || res.statusText);
  }
  return res.json();
}

export const api = {
  searchAnime: (q, limit) => request('/anime/search', { q, limit }),
  getAnime: (title) => request(`/anime/${encodeURIComponent(title)}`),
  getAnimeCharacters: (title, skip, limit) =>
    request(`/anime/${encodeURIComponent(title)}/characters`, { skip, limit }),
  getRelatedByStudio: (title, skip, limit) =>
    request(`/anime/${encodeURIComponent(title)}/related-by-studio`, { skip, limit }),
  getRelatedByStaff: (title, skip, limit) =>
    request(`/anime/${encodeURIComponent(title)}/related-by-staff`, { skip, limit }),

  searchCharacter: (q, limit) => request('/character/search', { q, limit }),
  getVoiceActors: (name, skip, limit) =>
    request(`/character/${encodeURIComponent(name)}/voice-actors`, { skip, limit }),
  getRelatedByVoiceActor: (name, skip, limit) =>
    request(`/character/${encodeURIComponent(name)}/related-by-voice-actor`, { skip, limit }),
  getCharacterPath: (from, to) => request('/character/path', { from, to }),

  getRelations: (name, skip, limit) => request('/relations', { name, skip, limit }),
};
