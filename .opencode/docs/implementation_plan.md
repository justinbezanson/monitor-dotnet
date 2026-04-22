# Implementation Plan: URL Monitoring Feature

This document outlines the steps required to implement the URL monitoring feature in the `MonitorDotNet` application.

## 1. Data Model Changes

We need to store the monitor configurations and their check history in the database.

### 1.1 `Monitor` Entity
Represents a URL being monitored.
- `Id`: Guid (Primary Key)
- `UserId`: string (Foreign Key to `AspNetUsers`)
- `Name`: string (Friendly name for the monitor)
- `Url`: string (The URL to check)
- `IntervalSeconds`: int (Interval between checks, default e.g., 60s)
- `IsEnabled`: bool (Whether monitoring is active)
- `LastCheckedAt`: DateTime? (Timestamp of the last check)
- `CurrentStatus`: string (Online, Offline, or Warning)

### 1.2 `MonitorCheck` Entity
Represents the result of a single check.
- `Id`: Guid (Primary Key)
- `MonitorId`: Guid (Foreign Key to `Monitors`)
- `Timestamp`: DateTime (When the check occurred)
- `IsSuccess`: bool (True if the request was successful)
- `StatusCode`: int? (HTTP status code returned)
- `ResponseTimeMs`: long (Latency in milliseconds)
- `ErrorMessage`: string? (Exception message if the check failed)

## 2. Backend Implementation (WebApi)

### 2.1 Entity Framework Core Setup
- Create `Monitor.cs` and `MonitorCheck.cs` in `WebApi/Data/Entities/`.
- Update `ApplicationDbContext.cs` to include `DbSet<Monitor>` and `DbSet<MonitorCheck>`.
- Configure relationships (User has many Monitors, Monitor has many Checks).
- Generate and apply a new EF Core migration.

### 2.2 API Endpoints
Implement the following endpoints using the Modular Minimal API pattern (`IEndpoint`):
- `GET /api/monitors`: Returns all monitors for the authenticated user.
- `POST /api/monitors`: Adds a new monitor.
- `GET /api/monitors/{id}`: Returns details of a specific monitor and its recent checks.
- `PUT /api/monitors/{id}`: Updates monitor settings.
- `DELETE /api/monitors/{id}`: Removes a monitor and its history.

### 2.3 Background Monitoring Service
Implement a `BackgroundService` to perform the actual monitoring.
- **Service Name:** `UrlMonitorService`
- **Logic:**
    1. Every 10-30 seconds, query the database for enabled monitors where `LastCheckedAt` is null or older than the `IntervalSeconds`.
    2. For each monitor due for a check, use `IHttpClientFactory` to send an asynchronous request to the URL.
    3. Measure the response time and capture the status code.
    4. Create a new `MonitorCheck` record.
    5. Update the `Monitor` record's `LastCheckedAt` and `CurrentStatus`.
    6. Use a `IServiceScopeFactory` to resolve the `ApplicationDbContext` within the background service.

## 3. Frontend Implementation (WebClient)

### 3.1 API Service
- Update `WebClient/src/lib/api/` to include methods for interacting with the `/api/monitors` endpoints.

### 3.2 Dashboard View
- Replace mock data in `DashboardView.vue` with data fetched from the backend.
- Implement the "Add New Monitor" button using a dialog component (e.g., from Reka UI or a custom Tailwind modal).
- Implement "Delete" and "Toggle Enabled" actions for each monitor in the list.
- Show a simple chart or sparkline for response time history (optional but recommended).

### 3.3 State Management
- Use Pinia or Vue's `ref` to manage the list of monitors and handle loading/error states.

## 4. Security & Performance
- **Authorization:** Ensure users can only access and modify their own monitors.
- **Validation:** Validate URLs and intervals on both client and server.
- **Cleanup:** Implement a background task or policy to purge old `MonitorCheck` records (e.g., older than 30 days) to prevent the database from growing indefinitely.
