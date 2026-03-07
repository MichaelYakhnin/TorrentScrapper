Docker setup for TorrentScrapper

Build and run:

  docker-compose up --build

Access the app at: http://localhost:3000

Notes:
- Backend listens internally on port 8080; nginx in the frontend container proxies /api to backend:8080.
- Frontend production builds output to /usr/share/nginx/html and are served by nginx.
- Backend CORS is configured via appsettings.json; ensure AllowedOrigins includes your host if accessing directly.
