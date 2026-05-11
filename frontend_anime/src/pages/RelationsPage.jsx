import { useState, useEffect } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api } from '../services/api';

const PAGE_SIZE = 20;

export default function RelationsPage() {
  const { name } = useParams();
  const [relations, setRelations] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setPage(0);
  }, [name]);

  useEffect(() => {
    async function fetchRelations() {
      setLoading(true);
      setError(null);
      try {
        const data = await api.getRelations(name, page * PAGE_SIZE, PAGE_SIZE);
        setRelations(data.items);
        setTotal(data.total);
      } catch (err) {
        setError(err.message || 'Failed to load relations');
        setRelations([]);
      } finally {
        setLoading(false);
      }
    }
    fetchRelations();
  }, [name, page]);

  function itemLink(item) {
    if (item.type === 'ANIME') return `/anime/${encodeURIComponent(item.name)}`;
    if (item.type === 'CHARACTER') return `/character/${encodeURIComponent(item.name)}`;
    return `/relations/${encodeURIComponent(item.name)}`;
  }

  const totalPages = Math.ceil(total / PAGE_SIZE);

  return (
    <div className="section">
      <h1>{decodeURIComponent(name)}</h1>

      {loading && <p className="loading">Loading...</p>}
      {error && <p className="error">{error}</p>}

      {!loading && !error && relations.length > 0 && (
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
      )}

      {!loading && totalPages > 1 && (
        <div className="pagination">
          <button
            className="btn btn-outline"
            disabled={page === 0}
            onClick={() => setPage((p) => p - 1)}
          >
            Prev
          </button>
          <span>
            Page {page + 1} of {totalPages}
          </span>
          <button
            className="btn btn-outline"
            disabled={page >= totalPages - 1}
            onClick={() => setPage((p) => p + 1)}
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
