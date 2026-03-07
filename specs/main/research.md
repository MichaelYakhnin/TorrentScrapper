# Research: Torrent Metadata Browser

## Technology Stack Decisions

### Decision: Use Angular (latest) for the frontend

- **Rationale**: Aligns with the constitution requirement for an Angular frontend
  with standalone components and Angular Material. Angular offers first-class
  tooling, routing, and Material integration for the paginator and card-based UI.
- **Alternatives considered**:
  - React or Vue: Popular SPA frameworks but would violate the constitution,
    which explicitly specifies Angular.
  - Server-rendered UI only: Would not match the requirement for frontend-driven
    pagination and an Angular Material-based experience.

### Decision: Use ASP.NET Core Web API targeting .NET 10 for the backend

- **Rationale**: Matches the constitution requirement for a .NET 10 Web API
  backend with controller-based routing, structured logging, and DI.
- **Alternatives considered**:
  - Older .NET versions (.NET 8/9): Would not satisfy the explicit .NET 10
    constraint.
  - Non-.NET backends (Node, Python, etc.): Would diverge from the project
    constraints and reduce integration with C#-centric tooling.

### Decision: Use AngleSharp as the primary HTML parser

- **Rationale**: AngleSharp provides a robust, standards-compliant HTML parsing
  model that works well with modern DOM-like queries and is well-suited for
  scraping listing and detail pages.
- **Alternatives considered**:
  - HtmlAgilityPack: Also allowed by the constitution, but AngleSharp offers a
    more modern API and better CSS-like querying; can still be used if needed.
  - Regex-based parsing: Explicitly prohibited by the constitution and fragile
    against markup changes.

### Decision: Implement optional IMemoryCache for (site, page) results

- **Rationale**: Short-lived in-memory caching (5 minutes per site+page) reduces
  load on external sites, improves perceived performance, and is explicitly
  mentioned as an optional optimization in the constitution.
- **Alternatives considered**:
  - No caching: Simpler but increases external calls, especially when users
    repeatedly visit the same page.
  - Distributed cache (Redis, etc.): Overkill for this project scope and not
    required by the constitution.

### Decision: Apply a fixed delay between detail page requests

- **Rationale**: A small delay (approximately 200ms) between requests to detail
  pages reduces the chance of triggering rate limits or anti-bot mechanisms on
  target sites and aligns with the constitution's rate limiting guidance.
- **Alternatives considered**:
  - No delay: Faster but higher risk of being treated as abusive traffic.
  - Complex adaptive rate limiting: More sophisticated but unnecessary for the
    initial scope.

### Decision: Use structured logging across backend components

- **Rationale**: Structured logging (e.g., with ILogger and scopes) is mandated
  by the constitution and is essential for diagnosing scraping issues, network
  failures, and parsing changes over time.
- **Alternatives considered**:
  - Simple text logging or Console.WriteLine: Prohibited by the constitution and
    not sufficient for production observability.

## Open Questions (tracked but not blocking)

- Ideal default page size per site is constrained by each external site's own
  pagination model and cannot be configured by us; we simply mirror their
  structure.
- Whether to add more sites later is left to future features; the current design
  remains extensible via the parser strategy interface.
