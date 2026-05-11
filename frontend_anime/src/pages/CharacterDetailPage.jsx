import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../services/api';

const PAGE_SIZE = 20;

function usePaginatedData(fetchFn, name) {
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [loaded, setLoaded] = useState(false);

  async function load(pageNum) {
    setLoading(true);
    setError(null);
    try {
      const data = await fetchFn(name, pageNum * PAGE_SIZE, PAGE_SIZE);
      setItems(data.items);
      setTotal(data.total);
      setPage(pageNum);
    } catch (err) {
      setError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
      setLoaded(true);
    }
  }

  function loadIfNeeded() {
    if (!loaded) load(0);
  }

  return { items, total, page, loading, error, load, loadIfNeeded, loaded };
}

export default function CharacterDetailPage() {
  const { name } = useParams();
  const decodedName = decodeURIComponent(name);

  const [activeTab, setActiveTab] = useState('voiceActors');

  const voiceActors = usePaginatedData(api.getVoiceActors, decodedName);
  const related = usePaginatedData(api.getRelatedByVoiceActor, decodedName);
  const relations = usePaginatedData(api.getRelations, decodedName);

  useEffect(() => {
    voiceActors.loadIfNeeded();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [decodedName]);

  function handleTabChange(tab) {
    setActiveTab(tab);
    if (tab === 'voiceActors') voiceActors.loadIfNeeded();
    if (tab === 'related') related.loadIfNeeded();
    if (tab === 'relations') relations.loadIfNeeded();
  }

  function renderPagination(data) {
    const totalPages = Math.ceil(data.total / PAGE_SIZE);
    if (totalPages <= 1) return null;
    return (
      <div className="pagination">
        <button
          className="btn btn-outline"
          disabled={data.page === 0}
          onClick={() => data.load(data.page - 1)}
        >
          Previous
        </button>
        <span>
          Page {data.page + 1} of {totalPages}
        </span>
        <button
          className="btn btn-outline"
          disabled={data.page >= totalPages - 1}
          onClick={() => data.load(data.page + 1)}
        >
          Next
        </button>
      </div>
    );
  }

  function renderVoiceActors() {
    const { items, loading, error } = voiceActors;
    if (loading) return <p className="loading">Loading...</p>;
    if (error) return <p className="error">{error}</p>;
    return (
      <div className="section">
        <ul className="result-list">
          {items.map((va, i) => (
            <li key={i} className="card">
              <strong>{va.name}</strong> — {va.language}
            </li>
          ))}
        </ul>
        {renderPagination(voiceActors)}
      </div>
    );
  }

  function renderRelated() {
    const { items, loading, error } = related;
    if (loading) return <p className="loading">Loading...</p>;
    if (error) return <p className="error">{error}</p>;
    return (
      <div className="section">
        <ul className="result-list">
          {items.map((item, i) => (
            <li key={i} className="card">
              <Link to={`/character/${encodeURIComponent(item.characterName)}`}>
                {item.characterName}
              </Link>
              {' in '}
              <Link to={`/anime/${encodeURIComponent(item.animeTitle)}`}>
                {item.animeTitle}
              </Link>
              {' — '}
              <span className="tag">{item.role}</span>
            </li>
          ))}
        </ul>
        {renderPagination(related)}
      </div>
    );
  }

  function relationLink(item) {
    if (item.type === 'ANIME') {
      return `/anime/${encodeURIComponent(item.name)}`;
    }
    if (item.type === 'CHARACTER') {
      return `/character/${encodeURIComponent(item.name)}`;
    }
    return `/relations/${encodeURIComponent(item.name)}`;
  }

  function renderRelations() {
    const { items, loading, error } = relations;
    if (loading) return <p className="loading">Loading...</p>;
    if (error) return <p className="error">{error}</p>;
    return (
      <div className="section">
        <ul className="result-list">
          {items.map((item, i) => (
            <li key={i} className="card">
              <Link to={relationLink(item)}>{item.name}</Link>
              {' '}
              <span className="tag">{item.type}</span>
              {' — '}
              {item.relationshipType} ({item.relationshipRole})
            </li>
          ))}
        </ul>
        {renderPagination(relations)}
      </div>
    );
  }

  return (
    <div>
      <h1>{decodedName}</h1>

      <Link className="btn btn-outline" to={`/path?from=${encodeURIComponent(decodedName)}`}>
        Find Path
      </Link>

      <div className="tabs">
        <button
          className={`tab${activeTab === 'voiceActors' ? ' active' : ''}`}
          onClick={() => handleTabChange('voiceActors')}
        >
          Voice Actors
        </button>
        <button
          className={`tab${activeTab === 'related' ? ' active' : ''}`}
          onClick={() => handleTabChange('related')}
        >
          Related Characters
        </button>
        <button
          className={`tab${activeTab === 'relations' ? ' active' : ''}`}
          onClick={() => handleTabChange('relations')}
        >
          Relations
        </button>
      </div>

      {activeTab === 'voiceActors' && renderVoiceActors()}
      {activeTab === 'related' && renderRelated()}
      {activeTab === 'relations' && renderRelations()}
    </div>
  );
}
