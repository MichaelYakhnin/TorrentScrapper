<!--
Sync Impact Report
- Version change: 0.0.0 → 1.0.0
- Modified principles: N/A (initial creation)
- Added sections: Core Principles, Backend Architecture, Frontend Architecture, Scraping & Parsing Standards, Performance & Stability, Security & Compliance, Governance
- Removed sections: None
- Templates requiring updates:
  - ✅ .specify/templates/plan-template.md
  - ✅ .specify/templates/spec-template.md
  - ✅ .specify/templates/tasks-template.md
  - ✅ .specify/templates/agent-file-template.md
  - ✅ .specify/templates/checklist-template.md
- Follow-up TODOs:
  - TODO(RATIFICATION_DATE): Set the original constitution ratification date if known
-->

# Torrent Metadata Scraper Constitution

## Core Principles

### I. Layered Architecture & Separation of Concerns

All code MUST follow a clear layered architecture:

- Presentation Layer: Angular frontend (standalone components, Angular Material, router)
- API Layer: ASP.NET Core Web API controllers (no business logic)
- Application Services: Orchestrate use cases, validation, and interaction between
  controllers, parsers, and infrastructure
- Parsing Layer: HTML parsing and metadata extraction using approved libraries only
- Infrastructure Layer: HTTP clients, caching, logging, configuration, persistence (if any)

Business rules and parsing rules MUST NOT be implemented directly in controllers.
Controllers MUST delegate to application services and return DTOs only.

**Rationale**: Enforces the Single Responsibility Principle (SRP), makes testing easier,
keeps controllers thin, and enables independent evolution of UI, API, and parsing logic.

### II. Single-Page, Metadata-Only Scraping Boundary

Backend scraping operations are strictly limited to a single page per request:

- Each API call MUST fetch and parse exactly one listing page for the chosen site
- The frontend controls pagination and MUST pass the requested page number
- No background jobs, no multi-page crawling, and no data aggregation across pages
- The system MUST NOT download torrent files or extract magnet links
- Only metadata fields are allowed: name, torrent detail page URL, and optional image URL

**Rationale**: Constrains scope, reduces legal and ethical risk, simplifies resource
management, and ensures predictable load and latency characteristics.

### III. Asynchronous, Resilient Networking & Structured Logging

All network operations MUST be asynchronous and cancellation-aware:

- All I/O-bound work uses `async`/`await` and accepts a [`CancellationToken`](backend/src/Application/Abstractions/ITorrentParser.cs)
- No use of `.Result`, `.Wait()`, or other blocking constructs on async operations
- No static `HttpClient` instances; all HTTP access uses [`IHttpClientFactory`](backend/src/Infrastructure/Http/HttpClientConfiguration.cs)

The system MUST implement structured logging:

- Use a consistent logging abstraction (e.g., `ILogger<T>`) in controllers, services,
  and parsers
- Log key events and failures with structured properties (site, page, error type,
  HTTP status code)
- No `Console.WriteLine` or ad-hoc logging in production code

**Rationale**: Async with cancellation prevents thread starvation and improves scalability.
Structured logs enable observability and safe operations in production.

### IV. Standards-Driven HTML Parsing

HTML parsing MUST adhere to these rules:

- Only AngleSharp or HtmlAgilityPack are permitted for HTML parsing
- No regex-based HTML parsing
- No HTML parsing logic in controllers; only in dedicated parser classes implementing
  the shared parser interface
- Parsing code MUST tolerate missing elements and unexpected markup without crashing

Site-specific requirements:

- For `rutor.info` listing pages:
  - Select anchors containing `/torrent/`
  - For each result, extract: name and detail page URL
- For `rutor.info` detail pages:
  - Extract the first valid poster image URL
  - Exclude icons and small decorative images (e.g., by size or known classes)
  - Normalize relative URLs to absolute URLs
  - If no suitable image is found, set `imageUrl` to `null`

**Rationale**: Dedicated parsing logic using battle-tested libraries improves
robustness and maintainability while reducing breakage from HTML changes.

### V. Backend Architecture & Contracts (.NET 10 Web API)

The backend MUST:

- Target .NET 10 (latest ASP.NET Core Web API)
- Use controller-based routing (attribute or conventional) returning DTOs only
- Support `CancellationToken` in all public async methods (controllers, services,
  parsers, HTTP calls)
- Validate inputs:
  - `page >= 1` (reject invalid pages with a clear 400 response)
  - `site` is a supported enum value (e.g., `Rutor`, `Rutracker`)
- Configure CORS to allow the Angular frontend origin

HTTP client configuration MUST:

- Use `IHttpClientFactory` with named/typed clients per torrent site
- Set an identifiable `User-Agent` header for all outbound requests
- Configure reasonable timeouts for scraper calls

Error handling MUST cover:

- HTTP 403 and other non-success responses from target sites
- Network failures and timeouts
- Parsing failures (invalid HTML, changed markup, missing elements)

Errors MUST be logged with sufficient context and converted into stable API responses
(HTTP status + problem details or equivalent DTO), without leaking sensitive internals.

**Rationale**: Aligns with modern ASP.NET Core practices, keeps contracts stable,
ensures validation and error semantics are predictable for the frontend.

### VI. Frontend Architecture & UX (Angular)

The frontend MUST:

- Use the latest Angular with standalone components and Angular Material
- Use strict TypeScript mode and feature-based module/route structure
- Use Angular Router for navigation
- Use Angular `HttpClient` for all API calls (no direct `fetch` calls)
- Use RxJS operators and observables (no manual Promise chains for HTTP flows)

Pagination and UX rules:

- Use Angular Material paginator to control `page` state
- On page change, trigger an API request for the selected page and selected site
- Display a loading spinner while network calls are in-flight
- Show errors via a clear, user-visible error component or Material snackbar/dialog

UI presentation:

- Use a responsive grid layout (e.g., Angular Material cards in a grid list) for
  torrent results
- Each card MUST display: name and (if present) poster image
- Each card MUST provide a button or clickable area that opens the torrent detail
  page URL in a new browser tab

DOM constraints:

- No direct DOM scrapers or HTML parsing in Angular; the frontend consumes metadata
  DTOs only

**Rationale**: Enforces a modern Angular architecture, ensures responsive UX, and
keeps scraping logic strictly on the backend.

### VII. Performance, Stability & Caching

The system MUST prioritize responsiveness and stability:

- Avoid blocking threads in the request path; all I/O is async
- Avoid excessive allocations in tight loops; reuse parsing infrastructure where
  appropriate
- Dispose parsing documents and HTTP responses correctly

Rate limiting and caching:

- Implement a basic delay between detail page requests when scraping listings
  that require fetching individual detail pages
- Optional: Use `IMemoryCache` (or equivalent) with a 5-minute expiration keyed by
  `(site + page)` to reduce load on target sites and improve response times

**Rationale**: Simple rate limiting and short-lived caching reduce risk of overloading
external sites and improve perceived performance without complex infrastructure.

### VIII. Security, Compliance & Ethical Constraints

The system MUST:

- Never download torrent files or magnet links
- Never scrape beyond the requested page or perform hidden background crawling
- Respect robots.txt where applicable; when a restriction is detected, log a warning
  and avoid non-compliant scraping
- Send an identifiable `User-Agent` header for all outbound HTTP requests
- Avoid any deliberate bypass of anti-bot or anti-scraping protections

**Rationale**: Ensures ethical scraping behavior, reduces legal risk, and respects
third-party site policies.

### IX. Extensibility via Parser Strategy Pattern

Torrent site parsing MUST follow a Strategy Pattern with a shared contract:

- Define a common interface:

  - [`ITorrentParser`](backend/src/Application/Abstractions/ITorrentParser.cs):
    exposes `Task<List<TorrentDto>> ParsePageAsync(int page, CancellationToken ct)`

- Implementations:

  - [`RutorParser`](backend/src/Parsing/RutorParser.cs)
  - [`RutrackerParser`](backend/src/Parsing/RutrackerParser.cs)

Parser selection rules:

- Parser selection MUST be dynamic and extensible (e.g., keyed by enum or site id)
- Adding a new torrent source MUST be possible by:
  - Creating a new parser class implementing `ITorrentParser`
  - Registering it in DI with the site key
  - Without modifying existing parser implementations or core pagination logic

**Rationale**: Enforces the Open/Closed Principle (OCP) and makes adding new torrent
sources a low-risk, localized change.

### X. Code Quality & Testing Discipline

All code MUST adhere to modern engineering standards:

- Apply SOLID principles across services, parsers, and UI components
- Use Dependency Injection for all external dependencies (HTTP clients, parsers,
  caches, loggers)
- Avoid magic strings; prefer enums, constants, or configuration objects
- Use strongly typed models and DTOs for API contracts (backend ↔ frontend)
- No dead code, commented-out blocks, or temporary debug statements in committed code
- Exceptions MUST be handled meaningfully; catch only where you can add context or
  convert to a stable API error

Testing expectations:

- Prefer unit tests for application services and parsers
- Prefer integration tests for controller APIs
- Ensure that core scraping, parsing, and pagination flows have automated coverage
  where feasible

**Rationale**: High code quality reduces regressions, improves maintainability,
and supports safe evolution of the system.

## Additional Constraints & Workflow

### Development Workflow & Review Gates

- All feature plans and specs MUST include a "Constitution Check" section describing
  how they comply with this constitution
- Code reviews MUST verify adherence to:
  - Single-page-per-request scraping boundary
  - Layered architecture and parser strategy pattern
  - Asynchronous and cancellation-aware networking
  - Security and compliance constraints
- Any deviations MUST be documented with explicit justification and, where possible,
  a plan to return to constitutional compliance.

### Deployment & Environment Configuration

- The project MUST build without warnings and run locally without manual code edits
- Environment-specific settings (e.g., API base URLs, allowed origins, scraper
  timeouts, user agent) MUST be externalized into configuration files or environment
  variables
- The system SHOULD be Docker-ready with a documented containerization approach for
  both backend and frontend, even if Docker artifacts are optional

## Governance

The Torrent Metadata Scraper Constitution supersedes any prior undocumented
conventions for this project.

### Amendment Procedure

- Any proposed change to these principles MUST be captured in a PR that updates
  this constitution file
- The PR MUST include:
  - Description of the change and motivation
  - Expected impact on existing code and features
  - Version bump type (MAJOR, MINOR, PATCH) with reasoning
- Changes MUST be reviewed and approved by project maintainers before merge

### Versioning Policy

- **MAJOR**: Backward-incompatible governance changes, removal or redefinition
  of core principles
- **MINOR**: Addition of new principles or substantial expansion of existing ones
- **PATCH**: Clarifications, wording cleanups, and non-semantic refinements

Each amendment MUST update the version metadata and Sync Impact Report at the top
of this file.

### Compliance & Review

- New feature plans (`plan.md`), specs (`spec.md`), task lists (`tasks.md`), and
  checklists MUST reference and respect the active constitution
- Automated or manual checks SHOULD enforce that scraping boundaries, architectural
  layering, and security constraints are not violated
- Non-compliant code MUST NOT be merged without explicit, temporary exceptions
  documented in this file and in the relevant plan/spec.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE) | **Last Amended**: 2026-03-01
