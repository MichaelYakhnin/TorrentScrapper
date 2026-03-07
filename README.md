# Torrent Metadata Browser

A full-stack web application for browsing torrent metadata from multiple torrent sites (rutor.info and rutracker.org). Built with .NET 10 Web API backend and Angular frontend.

## Features

- ✅ Browse torrents by site (Rutor, Rutracker) and category
- ✅ Category-based browsing (e.g., "Зарубежные фильмы", "Наши сериалы")
- ✅ Pagination within each category
- ✅ View poster images when available
- ✅ Open torrent detail pages in new tab
- ✅ Comprehensive error handling with user-friendly messages
- ✅ Rate-limited scraping (200ms delay between detail page requests)
- ✅ 5-minute caching per site/category/page
- ✅ Responsive Angular Material UI

## Prerequisites

- .NET 10 SDK
- Node.js 18+ and npm
- Modern web browser

## Project Structure

```
TorrentScrapper/
├── backend/                    # .NET 10 Web API
│   ├── src/
│   │   ├── TorrentScrapper.Api/          # API controllers and middleware
│   │   ├── TorrentScrapper.Application/  # Business logic and services
│   │   ├── TorrentScrapper.Infrastructure/ # HTTP client configuration
│   │   └── TorrentScrapper.Parsing/      # Site-specific parsers
│   └── tests/
│       ├── TorrentScrapper.Tests.Unit/
│       └── TorrentScrapper.Tests.Integration/
├── frontend/                   # Angular application
│   └── src/
│       └── app/
│           ├── features/results/  # Results page
│           └── shared/           # Services and models
└── specs/                      # Specifications and documentation
```

## Quick Start

### 1. Run the Backend API

```bash
cd backend/src/TorrentScrapper.Api
dotnet restore
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5210`
- HTTPS: `https://localhost:7081`
- Swagger UI: `http://localhost:5210/swagger`

### 2. Run the Frontend

In a new terminal:

```bash
cd frontend
npm install
npm start
```

The frontend will start on `http://localhost:4200`

### 3. Access the Application

Open your browser to `http://localhost:4200`

## API Endpoints

### GET /api/torrents/categories

Fetch available categories for a specific site.

**Query Parameters:**
- `site` (required): `rutor` or `rutracker`

**Example:**
```
GET http://localhost:5210/api/torrents/categories?site=rutor
```

**Success Response (200):**
```json
{
  "site": "rutor",
  "categories": [
    {
      "id": "foreign-films",
      "name": "Зарубежные фильмы",
      "url": "kino"
    },
    {
      "id": "russian-films",
      "name": "Наши фильмы",
      "url": "kino"
    }
  ]
}
```

### GET /api/torrents

Fetch torrents for a specific site, category, and page.

**Query Parameters:**
- `site` (required): `rutor` or `rutracker`
- `category` (optional): Category URL path (e.g., `kino`, `seriali`, `f/7`)
- `page` (required): Page number (>= 1)

**Examples:**
```
# Browse all torrents
GET http://localhost:5210/api/torrents?site=rutor&page=1

# Browse specific category
GET http://localhost:5210/api/torrents?site=rutor&category=kino&page=1

# Rutracker category
GET http://localhost:5210/api/torrents?site=rutracker&category=f/7&page=1
```

**Success Response (200):**
```json
{
  "site": "rutor",
  "page": 1,
  "results": [
    {
      "name": "Example Torrent",
      "torrentPageUrl": "https://rutor.info/torrent/123456",
      "imageUrl": "https://example.com/poster.jpg"
    }
  ]
}
```

**Error Responses:**
- `400 Bad Request`: Invalid site or page parameter
- `503 Service Unavailable`: Network error reaching torrent site
- `500 Internal Server Error`: Parsing error

## Configuration

### Backend (appsettings.json)

```json
{
  "Scraping": {
    "UserAgent": "TorrentMetadataBrowser/1.0 (Educational purposes)",
    "RequestTimeout": 30,
    "DetailPageDelay": 200,
    "CacheExpirationMinutes": 5
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200"]
  }
}
```

### Frontend (environment.ts)

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5210'
};
```

## Running Tests

### Backend Tests

```bash
cd backend

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TorrentScrapper.Tests.Unit
dotnet test tests/TorrentScrapper.Tests.Integration
```

### Frontend Tests

```bash
cd frontend
npm test
```

## Build for Production

### Backend

```bash
cd backend/src/TorrentScrapper.Api
dotnet publish -c Release -o ./publish
```

### Frontend

```bash
cd frontend
npm run build
# Output will be in frontend/dist/frontend
```

## Architecture

### Backend Layers

1. **API Layer** (`TorrentScrapper.Api`)
   - Controllers with validation
   - Error handling middleware
   - CORS configuration

2. **Application Layer** (`TorrentScrapper.Application`)
   - Services with business logic
   - DTOs and contracts
   - Parser abstractions

3. **Parsing Layer** (`TorrentScrapper.Parsing`)
   - Site-specific parsers (RutorParser, RutrackerParser)
   - Detail page clients
   - HTML parsing with AngleSharp

4. **Infrastructure Layer** (`TorrentScrapper.Infrastructure`)
   - HTTP client factory configuration
   - Shared infrastructure concerns

### Frontend

- **Standalone Components** - Modern Angular architecture
- **Signal-based State** - Reactive state management
- **Angular Material** - UI components and theming
- **RxJS** - Async operations and data streams

## Key Design Decisions

### Scraping Ethics
- ✅ Identifiable User-Agent
- ✅ Rate limiting (200ms between requests)
- ✅ Metadata-only (no torrent downloads)
- ✅ Single-page-per-request model
- ✅ Caching to reduce load

### Error Handling
- ✅ Structured logging with ILogger
- ✅ User-friendly error messages
- ✅ Graceful degradation
- ✅ Comprehensive validation

### Performance
- ✅ 5-minute in-memory cache
- ✅ Async/await throughout
- ✅ CancellationToken support
- ✅ Lazy loading in frontend

## Development

### Adding a New Torrent Site

1. Create parser in `backend/src/TorrentScrapper.Parsing/[SiteName]/`
2. Implement `ITorrentParser` interface
3. Create detail client for image extraction
4. Register in DI container (`Program.cs`)
5. Add site to `TorrentSite` enum
6. Add to frontend site selector

### Technology Stack

**Backend:**
- .NET 10
- ASP.NET Core Web API
- AngleSharp (HTML parsing)
- xUnit (testing)
- Moq (mocking)

**Frontend:**
- Angular 21+
- Angular Material
- RxJS
- TypeScript
- SCSS

## Troubleshooting

### Frontend not loading data

1. Ensure backend is running on `http://localhost:5210`
2. Check browser console for CORS errors
3. Verify `environment.ts` has correct `apiBaseUrl`
4. Check browser network tab for API calls

### Backend failing to parse

1. Check if target site is accessible
2. Verify User-Agent is set correctly
3. Review parsing selectors (sites may change structure)
4. Check logs for detailed error messages

### CORS issues

1. Verify frontend origin in `appsettings.json` CORS configuration
2. Ensure CORS middleware is enabled in `Program.cs`
3. Check browser console for specific CORS error

## License

Educational purposes only. Respect robots.txt and site terms of service.

## Contributing

This project follows clean architecture principles and SOLID design patterns. All contributions should:
- Include tests
- Follow existing code style
- Update documentation
- Respect scraping ethics
