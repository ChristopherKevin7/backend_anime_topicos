import { Link, Outlet } from 'react-router-dom';

export default function Layout() {
  return (
    <div className="app">
      <header className="header">
        <Link to="/" className="logo">Anime Graph</Link>
        <nav>
          <Link to="/">Search</Link>
          <Link to="/path">Character Path</Link>
        </nav>
      </header>
      <main className="main">
        <Outlet />
      </main>
    </div>
  );
}
