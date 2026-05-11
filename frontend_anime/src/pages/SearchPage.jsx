import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';
import AnimeGraph from '../components/AnimeGraph';
import { buildGraphData } from '../utils/graphUtils';

export default function SearchPage() {
  const [query, setQuery] = useState('');
  const [mode, setMode] = useState('anime');
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [centerNode, setCenterNode] = useState(null);
  const [graphData, setGraphData] = useState({ nodes: [], links: [] });
  const [relationsLoading, setRelationsLoading] = useState(false);
  const [relationsError, setRelationsError] = useState(null);
  const [viewMode, setViewMode] = useState('graph');

  useEffect(() => {
    if (!query.trim()) {
      setResults([]);
      setCenterNode(null);
      setGraphData({ nodes: [], links: [] });
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
        setCenterNode(data.length > 0 ? data[0] : null);
      } catch (err) {
        setError(err.message || 'Search failed');
        setResults([]);
        setCenterNode(null);
      } finally {
        setLoading(false);
      }
    }, 300);

    return () => clearTimeout(timer);
  }, [query, mode]);

  useEffect(() => {
    if (!centerNode) {
      setGraphData({ nodes: [], links: [] });
      return;
    }

    let cancelled = false;
    setRelationsLoading(true);
    setRelationsError(null);

    api.getRelations(centerNode, 0, 50)
      .then((data) => {
        if (!cancelled) setGraphData(buildGraphData(centerNode, mode === 'anime' ? 'ANIME' : 'CHARACTER', data.items));
      })
      .catch((err) => {
        if (!cancelled) setRelationsError(err.message || 'Failed to load relations');
      })
      .finally(() => {
        if (!cancelled) setRelationsLoading(false);
      });

    return () => { cancelled = true; };
  }, [centerNode, mode]);

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

      {(loading || relationsLoading) && <p className="loading">Loading...</p>}
      {error && <p className="error">{error}</p>}

      {centerNode && !loading && (
        <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 8, gap: 4 }}>
          <button
            className={`tab${viewMode === 'graph' ? ' active' : ''}`}
            onClick={() => setViewMode('graph')}
          >
            Graph
          </button>
          <button
            className={`tab${viewMode === 'list' ? ' active' : ''}`}
            onClick={() => setViewMode('list')}
          >
            List
          </button>
        </div>
      )}

      {!loading && !relationsLoading && !error && viewMode === 'graph' && graphData.nodes.length > 0 && (
        <AnimeGraph graphData={graphData} />
      )}

      {!loading && !error && (viewMode === 'list' || relationsError) && results.length > 0 && (
        <>
          {relationsError && <p className="error">{relationsError}</p>}
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
        </>
      )}
    </div>
  );
}
