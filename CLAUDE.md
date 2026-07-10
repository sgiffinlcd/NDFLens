# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Project Overview

NDFLens is a log viewer application for logs generated with the NDF logging extensions. It is a .NET 10 Blazor Server app (global InteractiveServer render mode) using MudBlazor as the UI component library, orchestrated by .NET Aspire, storing log data in SQL Server accessed via Dapper. The CodeFactory.NDF framework is used across all projects. The solution is currently in the architecture-setup phase — most layer libraries contain placeholder stubs.

## Build / Run

The solution file is `src/app/NDFLens/NDFLens.slnx` (XML `.slnx` format).

```
# Build everything
dotnet build src/app/NDFLens/NDFLens.slnx

# Run (recommended) — starts the SQL Server container, web app, and Aspire dashboard.
# Requires Docker to be running.
dotnet run --project src/app/NDFLens/NDFLens.AppHost/NDFLens.AppHost.csproj

# Watch mode
dotnet watch run --project src/app/NDFLens/NDFLens.AppHost/NDFLens.AppHost.csproj
```

There are no test projects yet.

## Architecture

The solution uses numbered solution folders with a strict layered layout. Each layer exposes a `*.Contracts` assembly (interfaces plus `Model.App` types only); implementations depend only on the next layer down's Contracts. Dependency direction: Web → Abstraction → Logic → Data.

| Layer | Projects | Role |
|---|---|---|
| 0-Hosting | `NDFLens.AppHost`, `NDFLens.ServiceDefaults` | Aspire orchestrator; shared OTel/health/resilience defaults |
| 1-Global | `NDFLens.Model.App` | POCO models used throughout the application — referenced by everything, references nothing |
| 2-UserInterface | `NDFLens.Web` | Blazor Server entry point (MudBlazor UI, global InteractiveServer render mode, self-hosted Roboto fonts); references all layers |
| 3-Abstraction | `NDFLens.Abstraction.Contracts`, `NDFLens.Abstraction.Direct` | Abstraction layer consumed by the UI |
| 4-Service | *(reserved, empty)* | Future service layer |
| 5-Logic | `NDFLens.Logic.Contracts`, `NDFLens.Logic` | Business logic |
| 6-Data | `NDFLens.Data.Contracts`, `NDFLens.Data.Sql` | Data access (Dapper + Microsoft.Data.SqlClient) |

Place new code in the correct layer: POCO models in `Model.App`, interfaces in the layer's `*.Contracts` project, implementations in the layer's implementation project.

## Dependency Injection — CodeFactory.NDF LibraryLoader

Each implementation library has a `LibraryLoader : DependencyInjectionLoader` (from CodeFactory.NDF) with three overrides:

- `LoadLibraries` — cascades loading to child libraries' loaders
- `LoadManualRegistration` — hand-written service registrations
- `LoadRegistration` — auto-generated registrations; **do not modify**

The Web project's `LibraryLoader` chains the child loaders: `Data.Sql` → `Logic` → `Abstraction.Direct`. Register new services in the owning library's `LibraryLoader.LoadManualRegistration`, never directly in `Program.cs`.

Known gap (current state, intentional): `NDFLens.Web/Program.cs` does not yet invoke the `LibraryLoader` — the DI cascade is defined but not wired into the host builder.

## Aspire / SQL Server

- The AppHost provisions a persistent SQL Server container `SQLNDFLens` (host port 53000, data volume `SQLNDFLens-data`) with database `NDFLogs`; the web front-end waits for the database.
- The SQL password comes from the Aspire secret parameter `sql-password` (user secrets on the AppHost project). Aspire injects the connection string at runtime as `ConnectionStrings:NDFLogs` — never hard-code connection strings in appsettings files.
- Health endpoints `/health` and `/alive` are mapped by `ServiceDefaults`.

## Conventions

- All projects target `net10.0` with `Nullable` and `ImplicitUsings` enabled.
- CodeFactory.NDF-generated files use block-scoped namespaces with XML doc comments — match that style in the layer libraries.
- No central package management; versions are pinned per csproj (`CodeFactory.NDF` 10.26141.1 everywhere).
