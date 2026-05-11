import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../services/api';

const PAGE_SIZE = 20;

export default function AnimeDetailPage() {
  const { title } = useParams();

  const [anime, setAnime] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [activeTab, setActiveTab] = useState('characters');

  const [characters, setCharacters] = useState({ items: [], total: 0 });
  const [charsSkip, setCharsSkip] = useState(0);
  const [charsLoading, setCharsLoading] = useState(false);

  const [relStudio, setRelStudio] = useState({ items: [], total: 0 });
  const [studioSkip, setStudioSkip] = useState(0);
  const [studioLoading, setStudioLoading] = useState(false);

  const [relStaff, setRelStaff] = useState({ items: [], total: 0 });
  const [staffSkip, setStaffSkip] = useState(0);
  const [staffLoading, setStaffLoading] = useState(false);

  const [tabError, setTabError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    api.getAnime(title)
      .then((data) => {
        if (!cancelled) setAnime(data);
      })
      .catch((err) => {
        if (!cancelled) setError(err.message || 'Failed to load anime details');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => { cancelled = true; };
  }, [title]);

  useEffect(() => {
    if (!anime) return;
    let cancelled = false;
    setTabError(null);

    if (activeTab === 'characters') {
      setCharsLoading(true);
      api.getAnimeCharacters(title, charsSkip, PAGE_SIZE)
        .then((data) => { if (!cancelled) setCharacters(data); })
        .catch((err) => { if (!cancelled) setTabError(err.message); })
        .finally(() => { if (!cancelled) setCharsLoading(false); });
    } else if (activeTab === 'studio') {
      setStudioLoading(true);
      api.getRelatedByStudio(title, studioSkip, PAGE_SIZE)
        .then((data) => { if (!cancelled) setRelStudio(data); })
        .catch((err) => { if (!cancelled) setTabError(err.message); })
        .finally(() => { if (!cancelled) setStudioLoading(false); });
    } else if (activeTab === 'staff') {
      setStaffLoading(true);
      api.getRelatedByStaff(title, staffSkip, PAGE_SIZE)
        .then((data) => { if (!cancelled) setRelStaff(data); })
        .catch((err) => { if (!cancelled) setTabError(err.message); })
        .finally(() => { if (!cancelled) setStaffLoading(false); });
    }

    return () => { cancelled = true; };
  }, [anime, activeTab, title, charsSkip, studioSkip, staffSkip]);

  if (loading) return <p className="loading">Loading...</p>;
  if (error) return <p className="error">{error}</p>;
  if (!anime) return null;

  const tabLoading =
    (activeTab === 'characters' && charsLoading) ||
    (activeTab === 'studio' && studioLoading) ||
    (activeTab === 'staff' && staffLoading);

  return (
    <div className="card">
      <section className="section">
        <h1>
          {anime.title}{' '}
          <small>
            Score: {anime.score} | Episodes: {anime.episodes} | {anime.animeType}
          </small>
        </h1>
      </section>

      {anime.genres && anime.genres.length > 0 && (
        <section className="section">
          {anime.genres.map((g) => (
            <span className="tag" key={g}>{g}</span>
          ))}
        </section>
      )}

      <div className="detail-grid">
        <section className="section">
          <h3>Studios</h3>
          <ul>
            {(anime.studios || []).map((s) => <li key={s}>{s}</li>)}
          </ul>
        </section>
        <section className="section">
          <h3>Producers</h3>
          <ul>
            {(anime.producers || []).map((p) => <li key={p}>{p}</li>)}
          </ul>
        </section>
        <section className="section">
          <h3>Licensors</h3>
          <ul>
            {(anime.licensors || []).map((l) => <li key={l}>{l}</li>)}
          </ul>
        </section>
      </div>

      <div className="tabs">
        <button
          className={`tab${activeTab === 'characters' ? ' active' : ''}`}
          onClick={() => { setActiveTab('characters'); setCharsSkip(0); }}
        >
          Characters
        </button>
        <button
          className={`tab${activeTab === 'studio' ? ' active' : ''}`}
          onClick={() => { setActiveTab('studio'); setStudioSkip(0); }}
        >
          Related by Studio
        </button>
        <button
          className={`tab${activeTab === 'staff' ? ' active' : ''}`}
          onClick={() => { setActiveTab('staff'); setStaffSkip(0); }}
        >
          Related by Staff
        </button>
      </div>

      {tabError && <p className="error">{tabError}</p>}
      {tabLoading && <p className="loading">Loading...</p>}

      {!tabLoading && !tabError && activeTab === 'characters' && (
        <>
          <ul className="result-list">
            {characters.items.map((ch) => (
              <li key={`${ch.name}-${ch.role}`}>
                <Link to={`/character/${encodeURIComponent(ch.name)}`}>{ch.name}</Link>
                {' '}&mdash; {ch.role}
              </li>
            ))}
          </ul>
          <Pagination
            skip={charsSkip}
            total={characters.total}
            pageSize={PAGE_SIZE}
            onPageChange={setCharsSkip}
          />
        </>
      )}

      {!tabLoading && !tabError && activeTab === 'studio' && (
        <>
          <ul className="result-list">
            {relStudio.items.map((r) => (
              <li key={r.title}>
                <Link to={`/anime/${encodeURIComponent(r.title)}`}>{r.title}</Link>
                {' '}&mdash; Score: {r.score} | {r.animeType} | Shared: {r.sharedEntity}
              </li>
            ))}
          </ul>
          <Pagination
            skip={studioSkip}
            total={relStudio.total}
            pageSize={PAGE_SIZE}
            onPageChange={setStudioSkip}
          />
        </>
      )}

      {!tabLoading && !tabError && activeTab === 'staff' && (
        <>
          <ul className="result-list">
            {relStaff.items.map((r) => (
              <li key={`${r.title}-${r.sharedEntity}`}>
                <Link to={`/anime/${encodeURIComponent(r.title)}`}>{r.title}</Link>
                {' '}&mdash; Score: {r.score} | {r.animeType} | Shared: {r.sharedEntity}
                {r.sharedEntityRole ? ` (${r.sharedEntityRole})` : ''}
              </li>
            ))}
          </ul>
          <Pagination
            skip={staffSkip}
            total={relStaff.total}
            pageSize={PAGE_SIZE}
            onPageChange={setStaffSkip}
          />
        </>
      )}
    </div>
  );
}

function Pagination({ skip, total, pageSize, onPageChange }) {
  if (total <= pageSize) return null;
  const page = Math.floor(skip / pageSize) + 1;
  const totalPages = Math.ceil(total / pageSize);

  return (
    <div className="pagination">
      <button
        className="btn btn-outline"
        disabled={skip === 0}
        onClick={() => onPageChange(skip - pageSize)}
      >
        Previous
      </button>
      <span>Page {page} of {totalPages}</span>
      <button
        className="btn btn-outline"
        disabled={skip + pageSize >= total}
        onClick={() => onPageChange(skip + pageSize)}
      >
        Next
      </button>
    </div>
  );
}
