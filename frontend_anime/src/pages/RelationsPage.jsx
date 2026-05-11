import { useState, useEffect } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api } from '../services/api';
import AnimeGraph from '../components/AnimeGraph';
import { buildGraphData } from '../utils/graphUtils';

const PAGE_SIZE = 20;

export default function RelationsPage() {
  const { name } = useParams();
  const [relations, setRelations] = useState([]);
  const [graphData, setGraphData] = useState({ nodes: [], links: [] });
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [viewMode, setViewMode] = useState('graph');

  useEffect(() => {
    setPage(0);
  }, [name]);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    const graphFetch = api.getRelations(name, 0, 50);
    const listFetch = api.getRelations(name, page * PAGE_SIZE, PAGE_SIZE);

    Promise.all([graphFetch, listFetch])
      .then(([graphRes, listRes]) => {
        if (cancelled) return;
        setGraphData(buildGraphData(decodeURIComponent(name), 'DEFAULT', graphRes.items));
        setRelations(listRes.items);
        setTotal(listRes.total);
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err.message || 'Failed to load relations');
          setRelations([]);
          setGraphData({ nodes: [], links: [] });
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, [name, page]);

  function itemLink(item) {
    if (item.type === 'ANIME') return `/anime/${encodeURIComponent(item.name)}`;
    if (item.type === 'CHARACTER') return `/character/${encodeURIComponent(item.name)}`;
    return `/relations/${encodeURIComponent(item.name)}`;
  }

  const totalPages = Math.ceil(total / PAGE_SIZE);
  const decodedName = decodeURIComponent(name);

  return (
    <div className="section">
      <h1>{decodedName}</h1>

      {loading && <p className="loading">Loading...</p>}
      {error && <p className="error">{error}</p>}

      {!loading && !error && (
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

      {!loading && !error && viewMode === 'graph' && graphData.nodes.length > 0 && (
        <AnimeGraph graphData={graphData} />
      )}

      {!loading && !error && viewMode === 'list' && relations.length > 0 && (
        <>
          <ul className="result-list">
            {relations.map((item, i) => (
              <li key={i} className="card">
                <Link to={itemLink(item)}>{item.name}</Link>
                <span className="tag">{item.type}</span>
                <span>{item.relationshipType}</span>
                {item.relationshipRole && <span> ({item.relationshipRole})</span>}
              </li>
            ))}
          </ul>

          {totalPages > 1 && (
            <div className="pagination">
              <button
                className="btn btn-outline"
                disabled={page === 0}
                onClick={() => setPage((p) => p - 1)}
              >
                Prev
              </button>
              <span>Page {page + 1} of {totalPages}</span>
              <button
                className="btn btn-outline"
                disabled={page >= totalPages - 1}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
