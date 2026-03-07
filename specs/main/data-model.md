# Data Model: Torrent Metadata Browser

## Overview

The system models torrent metadata fetched from external torrent listing sites
(rutor.info, rutracker.org) and exposes it via a simple, paginated API consumed
by the frontend.

## Core Entities

### TorrentSite (conceptual enum)

Represents the external torrent site being queried.

- **Values**:
  - `Rutor`
  - `Rutracker`

Used to:

- Validate incoming `site` requests
- Select the appropriate parser strategy implementation

### TorrentResult

Represents a single torrent entry returned to the frontend.

- **Fields**:
  - `name`: Human-readable torrent title extracted from the listing page.
  - `torrentPageUrl`: Absolute URL to the torrent detail page on the source site.
  - `imageUrl` (optional): Absolute URL to a poster-style image extracted from
    the detail page, or empty/null if no suitable image is found.

### PagedTorrentResponse

Represents the API response for a single page of torrents from a given site.

- **Fields**:
  - `site`: The logical site identifier (e.g., `rutor`, `rutracker`).
  - `page`: The requested page number (integer, must be >= 1).
  - `results`: Ordered collection of `TorrentResult` entries for that page.

### ErrorResponse (conceptual)

Represents error information returned by the API when a request cannot be
fulfilled.

- **Fields**:
  - `status`: High-level status or code (e.g., `validation_error`,
    `network_error`, `parsing_error`).
  - `message`: User-facing description of what went wrong.
  - `details` (optional): Additional structured information for logging or
    diagnostics (may be omitted from user-facing messages).

## Relationships

- A `PagedTorrentResponse` contains zero or more `TorrentResult` objects.
- Each `PagedTorrentResponse` is associated with exactly one `TorrentSite` via
  the `site` field.
- Error responses are returned instead of a `PagedTorrentResponse` when
  validation, networking, or parsing fails.

## Validation Rules (derived from spec)

- `site` must be one of the supported values; otherwise, a validation error is
  returned.
- `page` must be an integer greater than or equal to 1; otherwise, a validation
  error is returned.
- `torrentPageUrl` must be a valid absolute URL pointing to the selected site.
- `imageUrl`, if present, must be a valid absolute URL; if a suitable image
  cannot be found, it is left empty/null.
