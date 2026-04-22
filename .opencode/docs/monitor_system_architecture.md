# Monitor System Architecture

This document provides a detailed architectural view of the URL monitoring system within `MonitorDotNet`.

## 1. High-Level Design

The URL monitoring system is composed of four main layers:

1.  **Client (Vue 3/Vite):** The user interface for managing monitors and viewing real-time status.
2.  **API Layer (ASP.NET Core Minimal API):** The RESTful interface for monitor configuration and status retrieval.
3.  **Data Persistence Layer (EF Core/SQLite):** Stores monitor configurations and check history.
4.  **Background Processing Layer (BackgroundService):** An asynchronous worker that performs periodic health checks on configured URLs.

## 2. Component Interaction

### 2.1 Configuration Workflow
1.  **User action:** User adds a new URL through the `DashboardView.vue` component.
2.  **API Call:** The frontend sends a `POST /api/monitors` request with the URL and interval.
3.  **Persistence:** The `MonitorsEndpoint` validates the request and saves the new `Monitor` entity to the SQLite database via EF Core.

### 2.2 Monitoring Workflow
1.  **Scheduled Trigger:** The `UrlMonitorService` background worker wakes up periodically (e.g., every 15 seconds).
2.  **Selection:** It queries the database for active monitors due for a check.
3.  **Execution:** For each monitor, a non-blocking `HttpClient` request is initiated.
4.  **Data Recording:**
    -   A `MonitorCheck` record is created with the result (latency, status code, success/failure).
    -   The parent `Monitor` entity's `LastCheckedAt` and `CurrentStatus` fields are updated.
5.  **Concurrency:** Multiple checks should run concurrently using `Task.WhenAll` or a limited concurrency semaphore to ensure performance when many URLs are configured.

### 2.3 Status Retrieval Workflow
1.  **Data Fetch:** The `DashboardView.vue` component polls the `GET /api/monitors` endpoint (or uses WebSockets/SignalR in future iterations).
2.  **Aggregation:** The API returns the latest status and metrics for all monitors belonging to the user.
3.  **Visualization:** The UI updates the status badges, response time sparklines, and incident counts.

## 3. Key Services & Interfaces

### 3.1 `IMonitorService` (Optional but recommended)
Encapsulates business logic for managing monitors and performing checks, ensuring a clean separation between the background worker and API endpoints.

### 3.2 `IHttpClientFactory`
Used by the background worker to create `HttpClient` instances, preventing socket exhaustion and ensuring proper DNS handling.

## 4. Scalability & Performance Considerations
- **SQLite Limitations:** While suitable for a single-user or small-team application, heavy concurrent write loads from the background worker and API may require careful transaction management or a transition to a more robust database like PostgreSQL for larger scales.
- **Worker Concurrency:** To handle hundreds or thousands of URLs, the `UrlMonitorService` should use a `SemaphoreSlim` to limit the number of concurrent HTTP requests and avoid overwhelming the host's networking stack.
- **Log Retention:** `MonitorCheck` records will accumulate quickly. A data retention policy (e.g., "keep checks for 30 days") is essential.
