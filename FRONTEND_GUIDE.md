# Anime Graph API — Guia de Integração Frontend

Base URL: `http://localhost:5128`  
Swagger UI: `http://localhost:5128/swagger`

---

## Visão Geral da Arquitetura

O backend é uma API REST em **C# (.NET 8)** conectada a um banco de dados **Neo4j** (grafo). O grafo contém ~10.000 animes, ~40.000 personagens, dubladores, staff, estúdios, produtoras, licensors e gêneros, todos interligados por relacionamentos tipados.

O frontend deve ser organizado em **três áreas principais**:

| Área | Descrição |
|------|-----------|
| [Página Inicial — Busca](#1-página-inicial--busca) | Busca de animes e personagens com resultados clicáveis |
| [Visualização de Grafo](#2-visualização-de-grafo--endpoint-relations) | Exploração interativa de nós e relacionamentos |
| [Caminho entre Personagens](#3-caminho-entre-personagens) | Busca do menor caminho entre dois personagens |

---

## 1. Página Inicial — Busca

### 1.1 Busca de Animes

**Endpoint:** `GET /anime/search?q={termo}&limit={n}`

- Retorna uma lista de títulos que contêm o termo buscado (case-insensitive, busca parcial).
- Use para popular um campo de autocomplete ou lista de resultados.

**Exemplo de requisição:**
```
GET /anime/search?q=re:zero&limit=10
```

**Resposta:**
```json
[
  "Re:Zero kara Hajimeru Isekai Seikatsu",
  "Re:Zero kara Hajimeru Isekai Seikatsu 2nd Season",
  "Re:Zero kara Hajimeru Isekai Seikatsu 3rd Season"
]
```

**Fluxo sugerido:**
1. Usuário digita no campo de busca → dispara `GET /anime/search?q=...`
2. Exibe a lista de títulos retornados
3. Usuário clica em um título → navega para a página de detalhes do anime

---

### 1.2 Detalhes de um Anime

**Endpoint:** `GET /anime/{title}`

- Busca parcial + melhor match: encontra o anime cujo título mais se aproxima do parâmetro.
- Retorna todas as informações principais do anime.

**Exemplo de requisição:**
```
GET /anime/Re%3AZero%20kara%20Hajimeru%20Isekai%20Seikatsu%203rd%20Season
```

**Resposta:**
```json
{
  "title": "Re:Zero kara Hajimeru Isekai Seikatsu 3rd Season",
  "score": 9.05,
  "episodes": 25,
  "animeType": "TV",
  "genres": ["Action", "Drama", "Fantasy", "Suspense"],
  "studios": ["White Fox"],
  "producers": ["Kadokawa", "AT-X"],
  "licensors": ["Crunchyroll"],
  "staff": [
    { "name": "Watanabe, Masaharu", "role": "Director" },
    { "name": "Iizuka, Aya", "role": "Sound Director" }
  ]
}
```

**Campos disponíveis para exibição:**

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `title` | string | Título do anime |
| `score` | number \| null | Nota média (0–10) |
| `episodes` | integer \| null | Número de episódios |
| `animeType` | string \| null | `"TV"`, `"Movie"`, `"OVA"`, `"TV Special"`, etc. |
| `genres` | string[] | Lista de gêneros |
| `studios` | string[] | Estúdios de animação |
| `producers` | string[] | Produtoras |
| `licensors` | string[] | Distribuidoras (Crunchyroll, Funimation, etc.) |
| `staff` | `{name, role}[]` | Equipe de produção com função |

---

### 1.3 Personagens de um Anime

**Endpoint:** `GET /anime/{title}/characters?skip={n}&limit={n}`

- Retorna os personagens do anime com paginação.
- O campo `matchedTitle` informa qual anime foi encontrado (útil quando a busca é parcial).

**Exemplo de requisição:**
```
GET /anime/Re%3AZero%203rd/characters?skip=0&limit=50
```

**Resposta:**
```json
{
  "items": [
    { "name": "Natsuki, Subaru", "role": "Main" },
    { "name": "Emilia", "role": "Main" },
    { "name": "Rem", "role": "Supporting" }
  ],
  "total": 45,
  "skip": 0,
  "limit": 50,
  "matchedTitle": "Re:Zero kara Hajimeru Isekai Seikatsu 3rd Season"
}
```

**Campos do item:**

| Campo | Descrição |
|-------|-----------|
| `name` | Nome do personagem |
| `role` | `"Main"` ou `"Supporting"` |

**Campos da paginação:**

| Campo | Descrição |
|-------|-----------|
| `total` | Total de personagens do anime |
| `skip` | Offset atual |
| `limit` | Quantidade retornada |
| `matchedTitle` | Título exato do anime encontrado (presente quando a busca é parcial) |

---

### 1.4 Animes Relacionados por Estúdio

**Endpoint:** `GET /anime/{title}/related-by-studio?skip={n}&limit={n}`

- Lista outros animes produzidos pelo mesmo estúdio.
- Ordenados por score decrescente.

**Exemplo:**
```
GET /anime/Re%3AZero%203rd%20Season/related-by-studio?skip=0&limit=20
```

**Resposta:**
```json
{
  "items": [
    {
      "title": "Steins;Gate",
      "score": 9.07,
      "animeType": "TV",
      "sharedEntity": "White Fox"
    }
  ],
  "total": 38,
  "skip": 0,
  "limit": 20
}
```

---

### 1.5 Animes Relacionados por Staff

**Endpoint:** `GET /anime/{title}/related-by-staff?skip={n}&limit={n}`

- Lista outros animes que compartilham membros da equipe de produção.
- Inclui qual membro é compartilhado e a função dele no anime buscado.

**Resposta:**
```json
{
  "items": [
    {
      "title": "Odd Taxi",
      "score": 8.63,
      "animeType": "TV",
      "sharedEntity": "Iizuka, Aya",
      "sharedEntityRole": "Sound Director"
    }
  ],
  "total": 140,
  "skip": 0,
  "limit": 50
}
```

| Campo | Descrição |
|-------|-----------|
| `sharedEntity` | Nome do membro de staff compartilhado |
| `sharedEntityRole` | Função desse membro no anime pesquisado |

---

### 1.6 Busca de Personagens

**Endpoint:** `GET /character/search?q={termo}&limit={n}`

- Retorna lista de nomes de personagens que contêm o termo buscado.

**Exemplo:**
```
GET /character/search?q=rem&limit=10
```

**Resposta:**
```json
["Rem", "Remia", "Remy"]
```

---

### 1.7 Dubladores de um Personagem

**Endpoint:** `GET /character/{name}/voice-actors?skip={n}&limit={n}`

**Exemplo:**
```
GET /character/Rem/voice-actors?skip=0&limit=20
```

**Resposta:**
```json
{
  "items": [
    { "name": "Minase, Inori", "language": "Japanese" },
    { "name": "Brianna Knickerbocker", "language": "English" }
  ],
  "total": 2,
  "skip": 0,
  "limit": 20
}
```

---

### 1.8 Personagens Relacionados por Dublador

**Endpoint:** `GET /character/{name}/related-by-voice-actor?skip={n}&limit={n}`

- Lista outros personagens que compartilham o mesmo dublador.

**Resposta:**
```json
{
  "items": [
    {
      "characterName": "Hestia",
      "animeTitle": "Dungeon ni Deai wo Motomeru no wa Machigatteiru Darou ka",
      "role": "Main"
    }
  ],
  "total": 15,
  "skip": 0,
  "limit": 50
}
```

---

## 2. Visualização de Grafo — Endpoint `/relations`

### Endpoint

**`GET /relations?name={nome}&skip={n}&limit={n}`**

- Recebe o **nome exato** (case-insensitive) de **qualquer nó** do grafo: anime, personagem, dublador, staff, estúdio, gênero, etc.
- Retorna todos os nós vizinhos (distância 1) com o tipo de relacionamento.

**Exemplo:**
```
GET /relations?name=Rem&skip=0&limit=50
```

**Resposta:**
```json
{
  "items": [
    {
      "name": "Re:Zero kara Hajimeru Isekai Seikatsu",
      "type": "ANIME",
      "relationshipType": "FEATURES_CHARACTER",
      "relationshipRole": "Supporting"
    },
    {
      "name": "Minase, Inori",
      "type": "VOICE_ACTOR",
      "relationshipType": "VOICED_BY",
      "relationshipRole": null
    }
  ],
  "total": 6,
  "skip": 0,
  "limit": 50
}
```

**Campos do item:**

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `name` | string | Nome do nó vizinho |
| `type` | string | Tipo do nó: `ANIME`, `CHARACTER`, `VOICE_ACTOR`, `STAFF`, `STUDIO`, `PRODUCER`, `LICENSOR`, `GENRE` |
| `relationshipType` | string | Tipo do relacionamento: `FEATURES_CHARACTER`, `VOICED_BY`, `HAS_STAFF`, `PRODUCED_BY`, `HAS_GENRE` |
| `relationshipRole` | string \| null | Role do relacionamento quando aplicável (ex: `"Main"`, `"Director"`) |

---

### Como implementar a visualização de grafo

A ideia é uma tela com um **grafo interativo** onde cada nó é clicável e expande os seus vizinhos.

**Fluxo recomendado:**

```
1. Usuário pesquisa um nome (pode usar /anime/search ou /character/search)
2. Seleciona um item da lista → dispara GET /relations?name={nome}
3. Frontend renderiza o nó central + todos os vizinhos retornados como arestas
4. Usuário clica em qualquer nó vizinho → dispara GET /relations?name={nomeDoVizinho}
5. Os novos nós são adicionados ao grafo existente (expansão progressiva)
6. Repetir para qualquer nó
```

**Bibliotecas sugeridas para o grafo:**
- [Cytoscape.js](https://js.cytoscape.org/) — leve, flexível, sem dependências
- [React Flow](https://reactflow.dev/) — ótimo para React com interatividade
- [D3.js force simulation](https://d3js.org/) — total controle visual
- [Vis.js Network](https://visjs.github.io/vis-network/docs/network/) — simples e prático

**Sugestão de cores por tipo de nó:**

| Tipo | Cor sugerida |
|------|-------------|
| `ANIME` | Azul `#4A90D9` |
| `CHARACTER` | Verde `#7ED321` |
| `VOICE_ACTOR` | Laranja `#F5A623` |
| `STAFF` | Roxo `#9B59B6` |
| `STUDIO` | Vermelho `#E74C3C` |
| `PRODUCER` | Cinza `#95A5A6` |
| `LICENSOR` | Amarelo `#F1C40F` |
| `GENRE` | Rosa `#E91E63` |

**Label de aresta sugerida:**
- Se `relationshipRole` for não nulo: `"{relationshipType} ({relationshipRole})"`
- Senão: apenas `"{relationshipType}"`

> **Atenção:** Para nomes com caracteres especiais (dois-pontos, aspas, etc.), encode a URL corretamente. Exemplo: `Re:Zero` → `Re%3AZero`.

---

## 3. Caminho entre Personagens

**Endpoint:** `GET /character/path?from={nome1}&to={nome2}`

- Encontra o **caminho mais curto** no grafo entre dois personagens (máximo 10 saltos).
- Suporta busca parcial: usa o melhor match para cada nome.

**Exemplo:**
```
GET /character/path?from=Rem&to=Emilia
```

**Resposta:**
```json
{
  "nodes": [
    { "name": "Rem", "type": "CHARACTER" },
    { "name": "Re:Zero kara Hajimeru Isekai Seikatsu", "type": "ANIME" },
    { "name": "Emilia", "type": "CHARACTER" }
  ]
}
```

**404 quando não há caminho:**
```json
{ "message": "Nenhum caminho encontrado entre 'Rem' e 'Emilia'." }
```

---

### Como implementar a página de caminho

```
1. Dois campos de busca: "De" e "Até"
2. Cada campo usa /character/search para sugestões (autocomplete)
3. Ao clicar em "Buscar Caminho" → dispara GET /character/path?from=...&to=...
4. Exibe o resultado como uma linha de nós conectados (timeline ou grafo linear)
5. Cada nó exibe o nome e o tipo, com ícone/cor correspondente
6. Se 404, exibe mensagem "Nenhum caminho encontrado"
```

**Exemplo de visualização:**
```
[Rem] ──── [Re:Zero kara Hajimeru Isekai Seikatsu] ──── [Emilia]
CHARACTER              ANIME                            CHARACTER
```

---

## Regras Gerais de Integração

### Encoding de URL
Sempre encode o título/nome antes de colocar na URL:
```js
const encoded = encodeURIComponent(title);
fetch(`/anime/${encoded}`);
```

### Paginação
Todos os endpoints com lista suportam `skip` e `limit`. O campo `total` indica o total de resultados disponíveis, permitindo calcular o número de páginas:
```js
const totalPages = Math.ceil(total / limit);
```

### Busca parcial vs. exata
- `/anime/search` e `/character/search`: **busca parcial** — retornam lista de opções
- `GET /anime/{title}`, `GET /anime/{title}/characters`, etc.: **melhor match parcial** — retornam dados do anime/personagem mais próximo do termo
- `GET /relations?name=`: **match exato** (case-insensitive) — use o nome retornado pelos endpoints de busca

### Tratamento de erros

| Código | Significado |
|--------|-------------|
| `200` | Sucesso |
| `400` | Parâmetro obrigatório ausente |
| `404` | Recurso não encontrado |
| `500` | Erro interno (verificar logs do servidor) |

---

## Resumo dos Endpoints

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/anime/search?q=&limit=` | Busca animes por título parcial |
| GET | `/anime/{title}` | Detalhes completos de um anime |
| GET | `/anime/{title}/characters?skip=&limit=` | Personagens de um anime |
| GET | `/anime/{title}/related-by-studio?skip=&limit=` | Animes do mesmo estúdio |
| GET | `/anime/{title}/related-by-staff?skip=&limit=` | Animes com staff compartilhado |
| GET | `/character/search?q=&limit=` | Busca personagens por nome parcial |
| GET | `/character/{name}/voice-actors?skip=&limit=` | Dubladores de um personagem |
| GET | `/character/{name}/related-by-voice-actor?skip=&limit=` | Personagens com mesmo dublador |
| GET | `/character/path?from=&to=` | Caminho mais curto entre dois personagens |
| GET | `/relations?name=&skip=&limit=` | Todos os nós vizinhos de um nó (para o grafo) |
| GET | `/health` | Status da conexão com o banco |
