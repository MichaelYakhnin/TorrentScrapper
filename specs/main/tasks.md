# Tasks: Torrent Metadata Browser

**Input**: Design documents from `/specs/main/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are NOT mandated by the spec; include basic integration and unit tests selectively for critical flows.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create backend solution and project structure in `backend/` for .NET 10 Web API
- [x] T002 Create frontend Angular workspace and project in `frontend/` with Angular Material
- [x] T003 [P] Add solution-level README and update quickstart notes in `specs/main/quickstart.md` if needed

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Setup .NET 10 Web API project in `backend/src/` with controller-based architecture and CORS for Angular frontend
- [x] T005 [P] Configure HttpClientFactory with named clients, identifiable User-Agent, and timeouts in `backend/src/Infrastructure/Http/HttpClientConfiguration.cs`
- [x] T006 [P] Implement global error handling and structured logging pipeline in `backend/src/Api/Startup/LoggingAndErrors.cs`
- [x] T007 Create core DTOs and site enum for torrent metadata in `backend/src/Application/Contracts/TorrentDtos.cs`
- [x] T008 Implement `ITorrentParser` interface and register `RutorParser` and `RutrackerParser` in DI in `backend/src/Application/Abstractions/ITorrentParser.cs` and `backend/src/Parsing/`
- [x] T009 [P] Setup Angular Material, global theming, and layout shell in `frontend/src/app/app.config.ts` and `frontend/src/app/layout/`
- [x] T010 [P] Configure Angular HttpClient base URL and environment settings in `frontend/src/environments/`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Browse torrents by site and page (Priority: P1) 🎯 MVP

**Goal**: Let users select a torrent site and browse torrent metadata page by page, with frontend-controlled pagination.

**Independent Test**: User can select a site, load page 1, move to page N, and see only that page's torrents without errors.

### Implementation for User Story 1

- [x] T011 [US1] Implement backend `TorrentSite` enum and validation logic in `backend/src/Application/Contracts/TorrentSite.cs`
- [x] T012 [US1] Implement `PagedTorrentResponseDto` and `TorrentResultDto` models in `backend/src/Application/Contracts/TorrentDtos.cs`
- [x] T013 [US1] Implement `ITorrentService` application service to coordinate parser selection and responses in `backend/src/Application/Services/TorrentService.cs`
- [x] T014 [US1] Implement `TorrentsController` with `GET /api/torrents` using DTOs and CancellationToken in `backend/src/Api/Controllers/TorrentsController.cs`
- [x] T015 [US1] Implement rutor listing-page URL builder and listing HTML fetch in `backend/src/Parsing/Rutor/RutorListingClient.cs`
- [x] T016 [US1] Implement rutracker listing-page URL builder and listing HTML fetch in `backend/src/Parsing/Rutracker/RutrackerListingClient.cs`
- [x] T017 [US1] Implement `RutorParser` to extract torrent names and page URLs for a single page in `backend/src/Parsing/Rutor/RutorParser.cs`
- [x] T018 [US1] Implement `RutrackerParser` to extract torrent names and page URLs for a single page in `backend/src/Parsing/Rutracker/RutrackerParser.cs`
- [x] T019 [US1] Add backend unit tests for site/page validation and single-page behavior in `backend/tests/Unit/TorrentsValidationTests.cs`
- [x] T020 [US1] Add backend integration tests for `GET /api/torrents` happy path per site in `backend/tests/Integration/TorrentsControllerTests.cs`
- [x] T021 [P] [US1] Create Angular site selection + paginator view model (including loading state) in `frontend/src/app/features/results/results.state.ts`
- [x] T022 [P] [US1] Implement Angular API client service for `/api/torrents` in `frontend/src/app/shared/services/torrents-api.service.ts`
- [x] T023 [US1] Implement results page container component and template with Angular Material paginator, grid of cards, and loading spinner in `frontend/src/app/features/results/results.page.ts` and `frontend/src/app/features/results/results.page.html`
- [x] T024 [US1] Implement initial routing and navigation from home to results, including default route to results, in `frontend/src/app/app.routes.ts`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - View details and poster images (Priority: P2)

**Goal**: Show poster images when available and allow opening the original torrent detail page in a new tab.

**Independent Test**: For torrents with poster images, cards show the image and a button opens the detail page in a new browser tab.

### Implementation for User Story 2

- [x] T025 [US2] Extend `TorrentResultDto` with nullable `imageUrl` in `backend/src/Application/Contracts/TorrentDtos.cs`
- [x] T026 [US2] Implement rutor detail-page fetch and image extraction helper in `backend/src/Parsing/Rutor/RutorDetailClient.cs`
- [x] T027 [US2] Implement rutracker detail-page fetch and image extraction helper in `backend/src/Parsing/Rutracker/RutrackerDetailClient.cs`
- [x] T028 [US2] Update `RutorParser` to fetch detail pages with basic rate limiting and extract first meaningful poster image in `backend/src/Parsing/Rutor/RutorParser.cs`
- [x] T029 [US2] Update `RutrackerParser` to fetch detail pages with basic rate limiting and extract poster images in `backend/src/Parsing/Rutracker/RutrackerParser.cs`
- [x] T030 [US2] Optionally cache (site, page) responses for 5 minutes using IMemoryCache in `backend/src/Application/Services/TorrentService.cs`
- [x] T031 [P] [US2] Update Angular API client models to include `imageUrl` in `frontend/src/app/shared/models/torrent.models.ts`
- [x] T032 [P] [US2] Display poster images in Angular Material cards on results page in `frontend/src/app/features/results/results.page.html`
- [x] T033 [US2] Add "Open Torrent Page" button that opens `torrentPageUrl` in a new tab in `frontend/src/app/features/results/results.page.html`

**Checkpoint**: User Story 1 plus User Story 2 now provide full browsing with images and deep links.

---

## Phase 5: User Story 3 - Robust errors and edge cases (Priority: P3)

**Goal**: Provide clear, user-friendly error messages for validation failures, network issues, and parsing errors.

**Independent Test**: Simulated invalid inputs and upstream failures result in friendly error messages in the UI without crashes.

### Implementation for User Story 3

- [x] T034 [US3] Implement typed error response DTOs and mapping from internal errors in `backend/src/Application/Contracts/ErrorDtos.cs`
- [x] T035 [US3] Implement middleware or filters to map exceptions and upstream failures to API error responses in `backend/src/Api/Middleware/ErrorHandlingMiddleware.cs`
- [x] T036 [US3] Add handling for 400 (validation), 503 (network), 403 (access), and 500 (parsing) cases in `backend/src/Api/Controllers/TorrentsController.cs`
- [x] T037 [US3] Implement structured logging for error cases with site, page, and status in `backend/src/Application/Services/TorrentService.cs`
- [x] T038 [US3] Add Angular error model and service to interpret backend errors in `frontend/src/app/shared/services/error-handler.service.ts`
- [x] T039 [US3] Implement error display component/snackbar for global errors in `frontend/src/app/shared/components/error-banner/error-banner.component.ts`
- [x] T040 [US3] Wire error handling into results page flow so failed requests show messages instead of crashing in `frontend/src/app/features/results/results.page.ts`
- [x] T041 [US3] Add backend integration tests for error mappings in `backend/tests/Integration/TorrentsErrorTests.cs`

**Checkpoint**: All three user stories are independently testable with robust error handling.

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T042 [P] Add README section documenting architecture, scraping rules, and constraints in `README.md`
- [ ] T043 [P] Add logging configuration docs and examples in `docs/logging.md`
- [ ] T044 [P] Add basic performance checks or manual test script for typical flows in `docs/perf-checklist.md`
- [ ] T045 Run end-to-end manual validation flow following `specs/main/quickstart.md`
- [ ] T046 Final code cleanup, removal of dead code, and verification against constitution in all layers

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories.
- **User Stories (Phase 3–5)**: Depend on Foundational phase completion.
  - User Story 1 (P1) should be completed first (MVP).
  - User Story 2 (P2) builds on User Story 1’s data structures and endpoints.
  - User Story 3 (P3) enhances robustness but can be developed partly in parallel once basic flows exist.
- **Polish (Final Phase)**: Depends on all desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Setup + Foundational phases; provides core browsing capability.
- **User Story 2 (P2)**: Depends on User Story 1 data contracts and parsing infrastructure.
- **User Story 3 (P3)**: Depends on User Story 1 endpoint + User Story 2 parsing where applicable; focuses on error handling.

### Parallel Opportunities

- Setup tasks T001–T003 can largely run in parallel.
- Foundational tasks marked [P] (T005, T006, T009, T010) can run in parallel once T004 is underway.
- In User Story 1, backend and frontend tasks (e.g., T011–T020 vs T021–T024) can proceed in parallel after contracts are agreed.
- In User Story 2 and 3, tasks marked [P] can be parallelized across team members and files.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T003).
2. Complete Phase 2: Foundational (T004–T010).
3. Complete Phase 3: User Story 1 (T011–T024).
4. STOP and validate that users can select a site and browse pages of torrent metadata.

### Incremental Delivery

1. Deliver MVP (User Story 1).
2. Add User Story 2 (images and deep links) and validate.
3. Add User Story 3 (robust error handling) and validate.
4. Apply Phase N polish tasks.

### Notes

- All tasks follow `- [ ] TXXX [P?] [US?] Description with file path` format.
- Focus on keeping each user story independently testable.
- Ensure all layering, scraping, and security constraints from the constitution are honored during implementation.
