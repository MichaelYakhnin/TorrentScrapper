# API Contract: GET /api/torrents

## Endpoint

- **Method**: GET
- **Path**: `/api/torrents`

## Query Parameters

- `site` (string, required)
  - Allowed values: `rutor`, `rutracker`
- `page` (integer, required)
  - Must be >= 1

## Request Semantics

- Frontend controls pagination and calls this endpoint whenever the selected
  site or page changes.
- Backend scrapes exactly one listing page per request from the chosen site.

## Successful Response (200 OK)

```json
{
  "site": "rutor",
  "page": 1,
  "results": [
    {
      "name": "Example torrent name",
      "torrentPageUrl": "https://rutor.info/torrent/123456/example",
      "imageUrl": "https://rutor.info/images/posters/123456.jpg"
    }
  ]
}
```

- `imageUrl` may be `null` if no suitable poster image is found.

## Validation Errors (400 Bad Request)

Conditions:

- `site` is missing or not one of the allowed values.
- `page` is missing, not an integer, or less than 1.

Example payload:

```json
{
  "status": "validation_error",
  "message": "Invalid site or page parameter."
}
```

## Upstream Failure: Network / Site Unavailable (503 Service Unavailable)

Conditions:

- Target site is unreachable (DNS, connection timeout, etc.).
- Target site returns a 5xx error.

Example payload:

```json
{
  "status": "network_error",
  "message": "Unable to reach the selected torrent site. Please try again later."
}
```

## Upstream Failure: Forbidden / Blocked (handled but mapped appropriately)

Conditions:

- Target site returns 403 or similar access-denied responses.

Example payload (status code may be 403 or mapped to a generic error as
appropriate):

```json
{
  "status": "access_error",
  "message": "Access to the selected torrent site is currently restricted."
}
```

## Parsing Failures (500 Internal Server Error)

Conditions:

- HTML structure changes such that required elements cannot be reliably parsed.
- Unexpected parsing exceptions occur despite defensive checks.

Example payload:

```json
{
  "status": "parsing_error",
  "message": "We were unable to interpret the torrent listing for this page."
}
```

## General Notes

- Responses are metadata-only: there is no torrent file content or magnet link
  in any response.
- The contract is designed so that adding new sites later involves extending the
  allowed `site` values and supporting them via new parser strategies, without
  changing the overall response shape.
