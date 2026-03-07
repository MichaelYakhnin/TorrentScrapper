# Quickstart: Torrent Metadata Browser

## Prerequisites

- .NET 10 SDK installed
- Node.js and npm installed (for Angular frontend)
- Modern web browser

## High-Level Setup

1. Clone or open the repository locally.
2. Ensure environment configuration files specify:
   - Backend base URL and port
   - Allowed CORS origin for the Angular frontend
   - User-Agent string and timeouts for outbound HTTP scraping requests

## Running the Backend (ASP.NET Core Web API)

1. Navigate to the backend project directory (e.g., `backend/`).
2. Restore dependencies and build the project.
3. Run the Web API locally (e.g., using `dotnet run`).
4. Confirm the API is available by calling `GET /api/torrents` with test
   parameters (e.g., `site=rutor&page=1`).

## Running the Frontend (Angular)

1. Navigate to the frontend project directory (e.g., `frontend/`).
2. Install dependencies using your package manager (e.g., `npm install`).
3. Start the development server (e.g., `npm start` or `ng serve`).
4. Open the application in a browser at the configured URL.

## Basic Usage Flow

1. Open the application in your browser.
2. Choose a torrent site (rutor.info or rutracker.org) from the selector.
3. Use the paginator to move between pages.
4. For any torrent card, use the provided button to open the torrent detail page
   on the original site in a new tab.

## Expected Behavior

- Each page change triggers exactly one backend API call for that page.
- Results show torrent names, metadata, and an optional poster image if
  available.
- No torrent files or magnet links are exposed.
- Errors such as invalid pages or upstream site issues are shown as friendly
  messages in the UI.
