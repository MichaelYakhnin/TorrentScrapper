# Implementation Plan: Torrent Metadata Browser

**Branch**: `main` | **Date**: 2026-03-01 | **Spec**: [spec.md](specs/main/spec.md)

**Input**: Feature specification from `specs/main/spec.md`

**Note**: This plan is generated from the Torrent Metadata Browser feature spec and
the project constitution.

## Summary

Build a full-stack web application that lets users browse torrent metadata from
rutor.info and rutracker.org. The Angular frontend controls pagination and calls
a .NET 10 Web API backend, which scrapes exactly one page per request using
site-specific parser strategies and returns metadata-only results (name,
torrentPageUrl, optional imageUrl).

## Technical Context

**Language/Version**: Backend: C# (.NET 10); Frontend: TypeScript (Angular latest)
**Primary Dependencies**: ASP.NET Core Web API, Angular, Angular Material, RxJS,
AngleSharp or HtmlAgilityPack, IMemoryCache (optional), Polly (optional)
**Storage**: N/A (no persistent storage; all data comes from external torrent sites)
**Testing**: xUnit/NUnit (backend) and Jasmine/Karma or Jest (frontend)
**Target Platform**: Backend: Windows/Linux server; Frontend: modern desktop
and mobile browsers
**Project Type**: Web service + single-page web application
**Performance Goals**: Typical page load (metadata fetch + render) under 3 seconds
for 95% of requests under normal network conditions; single-page-per-request
scraping model to keep load predictable
**Constraints**: Single-page-per-request scraping, metadata-only (no torrent
downloads or magnet links), no background crawling, compliance with robots.txt
and ethical scraping practices
**Scale/Scope**: Two initial sources (rutor.info, rutracker.org); limited to
browsing top or listing pages with frontend-driven pagination

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Verify layered architecture (Angular frontend, .NET 10 Web API backend, application services, parsing, infrastructure)
- Verify single-page-per-request scraping boundary and metadata-only scope
- Verify use of async, cancellation-aware networking and IHttpClientFactory
- Verify parser strategy pattern (ITorrentParser + site-specific parsers) and no HTML parsing in controllers
- Verify Angular frontend uses standalone components, Angular Material, and Material paginator for pagination
- Verify security and compliance constraints (no torrent downloads/magnets, robots.txt awareness, identifiable User-Agent)

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
