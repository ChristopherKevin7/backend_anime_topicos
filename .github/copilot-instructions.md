## Estrutura do Banco de Dados em Grafo — Anime

---

### Nós (nodes.csv)

Cada nó tem um `id` (que é o próprio nome da entidade) e um `type`. Os tipos existentes são:

| type | Descrição |
|---|---|
| `anime` | Obra (série, filme, OVA). Tem: `title`, `score`, `episodes`, `anime_type` |
| `character` | Personagem de um anime. Tem: `name` |
| `voice_actor` | Dublador de um personagem. Tem: `name` |
| `staff` | Membro da equipe de produção (diretor, compositor, etc). Tem: `name` |
| `studio` | Estúdio de animação. Tem: `name` |
| `producer` | Produtora/financiadora. Tem: `name` |
| `licensor` | Distribuidora (ex: Funimation, Crunchyroll). Tem: `name` |
| `genre` | Gênero do anime (ex: Action, Romance). Tem: `name` |

---

### Relacionamentos (relationships.csv)

Cada aresta tem `source` (nome origem), `target` (nome destino), `type` e atributos opcionais:

| type | source → target | Atributos extras |
|---|---|---|
| `features_character` | Anime → Personagem | `role`: Main / Supporting / Unknown |
| `voiced_by` | Personagem → Voice Actor | `language`: Japanese / English / etc |
| `has_staff` | Anime → Staff | `role`: Director / Producer / etc |
| `produced_by` | Anime → Studio / Producer / Licensor | — |
| `has_genre` | Anime → Genre | — |

---

### Fluxo de dados (exemplo)

```
"Gintama°" (anime)
  ├─[features_character, role=Main]──► "Sakata, Gintoki" (character)
  │                                         └─[voiced_by, language=Japanese]──► "Sugita, Tomokazu" (voice_actor)
  ├─[has_staff, role=Director]──────► "Takamatsu, Shinji" (staff)
  ├─[produced_by]───────────────────► "Bandai Namco Pictures" (studio)
  └─[has_genre]─────────────────────► "Action" (genre)
```

---

### Volumes

- ~61.500 nós no total
- ~341.000 relacionamentos no total
- ~10.000 animes, ~40.000 personagens, ~76 gêneros

---

### Observações importantes para o backend

- O `id` de cada nó **é o próprio nome** (string), não um inteiro — então nas queries do Neo4j o match deve ser por `name` ou `title`, não por número
- Nomes com vírgula são comuns (ex: `"Sugita, Tomokazu"`) — tratar encoding UTF-8
- Alguns personagens sem nome no dataset aparecem como `"Unknown Character X"` — pode valer filtrar no backend
- A relação `produced_by` agrega estúdios, produtoras e licensors sob o mesmo tipo de aresta — se precisar distinguir, use o `type` do nó de destino