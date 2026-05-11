import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';

export default function SearchPage() {
  const [query, setQuery] = useState('');
  const [mode, setMode] = useState('anime');
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!query.trim()) {
      setResults([]);
      setError(null);
      return;
    }

    const timer = setTimeout(async () => {
      setLoading(true);
      setError(null);
      try {
        const data =
          mode === 'anime'
            ? await api.searchAnime(query, 20)
            : await api.searchCharacter(query, 20);
        setResults(data);
      } catch (err) {
        setError(err.message || 'Search failed');
        setResults([]);
      } finally {
        setLoading(false);
      }
    }, 300);

    return () => clearTimeout(timer);
  }, [query, mode]);

  return (
    <div>
      <input
        className="search-box"
        type="text"
        placeholder={`Search ${mode === 'anime' ? 'anime' : 'characters'}...`}
        value={query}
        onChange={(e) => setQuery(e.target.value)}
      />

      <div className="tabs">
        <button
          className={`tab${mode === 'anime' ? ' active' : ''}`}
          onClick={() => setMode('anime')}
        >
          Anime
        </button>
        <button
          className={`tab${mode === 'character' ? ' active' : ''}`}
          onClick={() => setMode('character')}
        >
          Character
        </button>
      </div>

      {loading && <p className="loading">Loading...</p>}
      {error && <p className="error">{error}</p>}

      {!loading && !error && results.length > 0 && (
        <ul className="result-list">
          {results.map((item) => (
            <li key={item}>
              <Link
                to={
                  mode === 'anime'
                    ? `/anime/${encodeURIComponent(item)}`
                    : `/character/${encodeURIComponent(item)}`
                }
              >
                {item}
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
