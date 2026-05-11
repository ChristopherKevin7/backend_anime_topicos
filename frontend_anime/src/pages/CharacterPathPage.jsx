import { useState, useEffect, useRef } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { api } from '../services/api';

export default function CharacterPathPage() {
  const [searchParams] = useSearchParams();

  const [from, setFrom] = useState(searchParams.get('from') || '');
  const [to, setTo] = useState(searchParams.get('to') || '');
  const [fromSuggestions, setFromSuggestions] = useState([]);
  const [toSuggestions, setToSuggestions] = useState([]);
  const [showFromDropdown, setShowFromDropdown] = useState(false);
  const [showToDropdown, setShowToDropdown] = useState(false);
  const [path, setPath] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const fromRef = useRef(null);
  const toRef = useRef(null);

  // Debounced autocomplete for "from"
  useEffect(() => {
    if (!from.trim()) {
      setFromSuggestions([]);
      return;
    }
    const timer = setTimeout(async () => {
      try {
        const data = await api.searchCharacter(from, 10);
        setFromSuggestions(data);
        setShowFromDropdown(true);
      } catch {
        setFromSuggestions([]);
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [from]);

  // Debounced autocomplete for "to"
  useEffect(() => {
    if (!to.trim()) {
      setToSuggestions([]);
      return;
    }
    const timer = setTimeout(async () => {
      try {
        const data = await api.searchCharacter(to, 10);
        setToSuggestions(data);
        setShowToDropdown(true);
      } catch {
        setToSuggestions([]);
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [to]);

  // Close dropdowns on outside click
  useEffect(() => {
    function handleClick(e) {
      if (fromRef.current && !fromRef.current.contains(e.target)) {
        setShowFromDropdown(false);
      }
      if (toRef.current && !toRef.current.contains(e.target)) {
        setShowToDropdown(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const handleFindPath = async () => {
    if (!from.trim() || !to.trim()) return;
    setLoading(true);
    setError(null);
    setPath(null);
    try {
      const data = await api.getCharacterPath(from, to);
      setPath(data.nodes);
    } catch (err) {
      if (err.message.includes('404') || err.message.toLowerCase().includes('not found')) {
        setError('No path found');
      } else {
        setError(err.message || 'Failed to find path');
      }
    } finally {
      setLoading(false);
    }
  };

  function nodeLink(node) {
    if (node.type === 'ANIME') return `/anime/${encodeURIComponent(node.name)}`;
    if (node.type === 'CHARACTER') return `/character/${encodeURIComponent(node.name)}`;
    return `/relations/${encodeURIComponent(node.name)}`;
  }

  return (
    <div className="section">
      <h1>Character Path Finder</h1>

      <div ref={fromRef} style={{ position: 'relative' }}>
        <input
          className="search-box"
          type="text"
          placeholder="From character"
          value={from}
          onChange={(e) => setFrom(e.target.value)}
          onFocus={() => fromSuggestions.length > 0 && setShowFromDropdown(true)}
        />
        {showFromDropdown && fromSuggestions.length > 0 && (
          <ul className="result-list" style={{ position: 'absolute', zIndex: 10, width: '100%' }}>
            {fromSuggestions.map((s) => (
              <li
                key={s}
                style={{ cursor: 'pointer' }}
                onClick={() => {
                  setFrom(s);
                  setShowFromDropdown(false);
                }}
              >
                {s}
              </li>
            ))}
          </ul>
        )}
      </div>

      <div ref={toRef} style={{ position: 'relative' }}>
        <input
          className="search-box"
          type="text"
          placeholder="To character"
          value={to}
          onChange={(e) => setTo(e.target.value)}
          onFocus={() => toSuggestions.length > 0 && setShowToDropdown(true)}
        />
        {showToDropdown && toSuggestions.length > 0 && (
          <ul className="result-list" style={{ position: 'absolute', zIndex: 10, width: '100%' }}>
            {toSuggestions.map((s) => (
              <li
                key={s}
                style={{ cursor: 'pointer' }}
                onClick={() => {
                  setTo(s);
                  setShowToDropdown(false);
                }}
              >
                {s}
              </li>
            ))}
          </ul>
        )}
      </div>

      <button className="btn" onClick={handleFindPath} disabled={loading}>
        {loading ? 'Searching...' : 'Find Path'}
      </button>

      {loading && <p className="loading">Loading...</p>}
      {error && <p className="error">{error}</p>}

      {path && path.length > 0 && (
        <div className="section">
          {path.map((node, i) => (
            <span key={i}>
              <span className="path-node">
                <Link to={nodeLink(node)}>{node.name}</Link>
                <span className="tag">{node.type}</span>
              </span>
              {i < path.length - 1 && <span className="path-arrow"> → </span>}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
