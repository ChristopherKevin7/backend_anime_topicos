import { Routes, Route } from 'react-router-dom'
import Layout from './components/Layout'
import SearchPage from './pages/SearchPage'
import AnimeDetailPage from './pages/AnimeDetailPage'
import CharacterDetailPage from './pages/CharacterDetailPage'
import CharacterPathPage from './pages/CharacterPathPage'
import RelationsPage from './pages/RelationsPage'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<SearchPage />} />
        <Route path="/anime/:title" element={<AnimeDetailPage />} />
        <Route path="/character/:name" element={<CharacterDetailPage />} />
        <Route path="/path" element={<CharacterPathPage />} />
        <Route path="/relations/:name" element={<RelationsPage />} />
      </Route>
    </Routes>
  )
}
