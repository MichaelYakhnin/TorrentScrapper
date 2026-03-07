TorrentScrapper
===============

Overview
--------
TorrentScrapper is a small torrent metadata browser with two components:
- backend: .NET 9 Web API (scrapers + API)
- frontend: Angular app served by nginx

Recent changes (summary)
------------------------
- Docker/docker-compose: added multi-service compose (frontend served by nginx on host port 3000, backend on 8080). nginx proxies /api/* to backend.
- CORS: backend CORS updated to allow http://localhost:3000 for Docker-hosted frontend.
- Frontend build fixes: corrected Dockerfile/nginx paths and removed duplicate '/api' prefix in environment config.
- Parsers:
  - RutorParser and RutrackerParser: limited selectors to the correct <td> cells, parallelized detail-page fetches using Parallel.ForEachAsync, and preserved result ordering (replaced unordered ConcurrentBag with indexed processing + ConcurrentDictionary).
  - RutrackerDetailClient: improved image extraction (handles <var.postImg title="...">, img src/data-src/srcset, anchor images and inline background-image; handles protocol-relative and root-relative URLs).
- Theme support:
  - Added a Light/Dark theme selector in the frontend and persisted selection to localStorage.
  - Initial attempt to use Angular Material SASS API caused build errors in CI/docker; switched to a runtime CSS-variable-based approach and added Material component overrides so theme switching works without SASS functions.

Known issues / pending work
--------------------------
- Rutracker image extraction: add debug logging to inspect failing pages and adjust heuristics if needed.
- Re-introduce scoped Angular Material SASS theming later if desired (requires aligning Material/Sass versions or build toolchain).
- Add unit/integration tests for parsers.

How to build and run (development)
----------------------------------
- Build and run both services locally with Docker Compose:
  docker compose up --build

- Rebuild and restart only the frontend service:
  docker compose build frontend && docker compose up -d frontend

- Backend: exposed on http://localhost:8080
- Frontend: exposed on http://localhost:3000 (nginx)

Key files changed
-----------------
- backend/src/TorrentScrapper.Parsing/Rutor/RutorParser.cs
- backend/src/TorrentScrapper.Parsing/Rutracker/RutrackerParser.cs
- backend/src/TorrentScrapper.Parsing/Rutracker/RutrackerDetailClient.cs
- backend/src/TorrentScrapper.Api/appsettings.json (CORS)
- frontend/src/app/app.ts, frontend/src/app/app.html (theme selector)
- frontend/src/styles.scss (runtime CSS-variable theming + Material overrides)
- frontend/nginx.conf, frontend/Dockerfile, docker-compose.yml (deployment)

Next steps
----------
- Add debug logging and re-run problematic Rutracker detail-page parsing to capture missing image cases.
- Add tests for parsers and CI checks.
- Optionally restore Angular Material SASS-based theming after resolving build-tool compatibility.
