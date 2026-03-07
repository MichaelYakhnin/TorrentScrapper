# Feature Specification: Torrent Metadata Browser

**Feature Branch**: `main`  
**Created**: 2026-03-01  
**Status**: Draft  
**Input**: User description: "Torrent Metadata Browser – browse torrent metadata from rutor.info and rutracker.org with frontend-controlled pagination and single-page-per-request scraping."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse torrents by site and page (Priority: P1)

A visitor wants to quickly see popular torrent metadata from a chosen site without
needing any torrent client or downloads.

**Why this priority**: This is the core value of the product – letting users see
relevant torrent metadata from supported sites.

**Independent Test**: A tester selects a site, loads page 1, and verifies a list
of torrent entries is shown with names and links only for that single page.

**Acceptance Scenarios**:

1. **Given** the user opens the application, **When** they select "rutor.info"
   and request page 1, **Then** they see a list of torrent entries from page 1
   for that site.
2. **Given** the user is viewing page N for a site, **When** they request page N+1
   from the paginator, **Then** the list updates to show entries only from page N+1
   and the page indicator reflects the new page.

---

### User Story 2 - View details and poster images (Priority: P2)

A visitor wants to understand what each torrent represents at a glance and open
more details in the original site when needed.

**Why this priority**: Images and clear links make it easier to decide which
entries are interesting and to navigate to the original source when desired.

**Independent Test**: A tester loads any page of results, confirms that card
entries show names, optional poster images, and that opening an entry loads the
corresponding detail page on the source site in a new tab.

**Acceptance Scenarios**:

1. **Given** a torrent entry has a poster image on its detail page, **When** the
   user views the results, **Then** the corresponding card shows that image.
2. **Given** a torrent entry does not have a suitable poster image, **When** the
   user views the results, **Then** the corresponding card shows no image area or
   a neutral placeholder, and no error is shown.
3. **Given** a torrent entry is shown in the list, **When** the user clicks the
   "Open Torrent Page" action, **Then** the torrent detail page opens in a new
   browser tab on the original torrent site.

---

### User Story 3 - Robust errors and edge cases (Priority: P3)

A visitor wants clear feedback when something goes wrong (e.g., the torrent site
is unavailable or a page does not exist), without the application crashing.

**Why this priority**: Clear error handling avoids confusion and makes the
application feel reliable even when external sites misbehave.

**Independent Test**: A tester simulates external failures (unreachable site,
forbidden access, invalid page number) and verifies that the UI displays clear,
actionable error messages while remaining usable.

**Acceptance Scenarios**:

1. **Given** the user enters an invalid page number, **When** they try to load it,
   **Then** the system rejects the request with a clear validation error message
   and does not call the backend.
2. **Given** the selected site is temporarily unavailable or returns an error,
   **When** the user tries to load a page, **Then** the UI shows a friendly error
   message indicating the problem and suggesting to retry later.
3. **Given** the backend cannot parse the external page for some reason,
   **When** the user tries to load that page, **Then** the UI shows a generic
   error for parsing issues without exposing technical details.

---

### Edge Cases

- Requested page number is less than 1.
- Requested page number is very large and returns no torrent entries.
- External site changes markup so some torrents or images cannot be parsed.
- External site responds with forbidden/unauthorized or rate-limit responses.
- Network timeouts or connectivity failures between the backend and external site.
- Robots.txt or similar restrictions make scraping a given path inappropriate.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow a user to choose one of the supported torrent
  sites and load metadata for page 1 when a site is selected.
- **FR-002**: The system MUST allow a user to change the page number and, for each
  change, load and display metadata only for that requested page.
- **FR-003**: For each torrent result, the system MUST provide at least a name
  and a link that navigates to the corresponding detail page on the source site.
- **FR-004**: For torrent entries that have a suitable poster image on the detail
  page, the system MUST attempt to show that image in the results; when no
  suitable image is found, the image field MUST be treated as empty.
- **FR-005**: The system MUST never download torrent files or expose magnet links;
  only descriptive metadata (name, source page URL, optional image URL) is
  returned and shown.
- **FR-006**: The system MUST validate the requested site and page number and
  refuse to process invalid combinations with clear error responses.
- **FR-007**: The system MUST provide clear, user-facing error messages when
  external sites are unreachable, respond with errors, or when parsing fails,
  without exposing internal technical details.

### Key Entities *(include if feature involves data)*

- **Torrent Result**: Represents a single torrent entry returned to the user,
  including name, source detail page URL, and optional image URL.
- **Site Selection**: Represents which external site is currently chosen (e.g.,
  rutor.info or rutracker.org) and influences which metadata is loaded.
- **Pagination State**: Represents the current page number for the selected site
  and any related information such as total known pages (if available).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view the first page of torrent metadata for a chosen site
  in under 3 seconds for at least 95% of attempts under normal network
  conditions.
- **SC-002**: At least 95% of valid page requests return a non-empty list of
  results when the external site has data for that page; empty pages are
  represented clearly when there are legitimately no results.
- **SC-003**: In at least 99% of cases where the external site returns a valid
  page, the application does not crash and instead either shows results or a
  controlled error message.
- **SC-004**: In usability testing, at least 90% of users are able to select a
  site, change pages, and open a torrent detail page without assistance.
- **SC-005**: No user-facing feature in the application allows downloading
  torrents or accessing magnet links; periodic reviews confirm only metadata is
  exposed.
