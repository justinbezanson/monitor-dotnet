# MonitorDotNet Architectural Overview

## 1. Introduction
`MonitorDotNet` is a web-based monitoring application built with a modern, high-performance stack. It follows a decoupled client-server architecture with a .NET-based backend and a Vue-powered frontend.

## 2. Technology Stack

### Backend: `WebApi` Project
- **Framework:** ASP.NET Core 10 (Web API)
- **Database:** SQLite, managed via Entity Framework Core (EF Core).
- **Authentication & Identity:** ASP.NET Core Identity with Bearer tokens (JWT) for stateless authorization.
- **API Documentation:** OpenAPI 3.1 with Scalar UI for a rich, interactive developer experience.
- **Coding Patterns:** Modular Minimal API pattern using `IEndpoint` interface and C# 11 `static abstract` members for clean registration.

### Frontend: `WebClient` Project
- **Framework:** Vue 3 (Composition API, `<script setup>`).
- **Build Tool:** Vite 8.
- **Language:** TypeScript.
- **Styling:** Tailwind CSS 4 (Beta).
- **UI Components:** Reka UI (headless primitives) and Lucide Vue Next icons.

## 3. Solution Structure
- **`/WebApi`**: The core backend service.
  - `Common/`: Interface definitions (`IEndpoint.cs`).
  - `Data/`: DB Context and Migrations.
  - `Endpoints/`: Central registration for modular endpoints.
  - `Status/`: Example feature for health checks.
- **`/WebClient`**: The frontend SPA.
  - `src/`: Vue components, logic, and styles.
  - `public/`: Static assets and icons.
- **`/.opencode`**: Project-specific documentation and agent configurations.

## 4. Key Architectural Decisions
- **Feature-Sliced Minimal APIs:** Endpoints are encapsulated by feature rather than controller, improving maintainability.
- **Shared Endpoint Interface:** Use of `IEndpoint` ensures a consistent registration pattern across the application.
- **Modern Styling:** Adoption of Tailwind CSS 4 for improved build performance and utility-first design.
- **Stateless Auth:** Implementation of Bearer tokens ensures the system remains scalable.
