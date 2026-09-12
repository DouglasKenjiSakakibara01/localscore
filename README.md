# LocalScore

Responsive web application for managing amateur football championships.

## Documentation

- [Implementation plan](docs/implementation-plan.md): stages, approved decisions, pending items, and completion criteria.
- [Agent instructions](AGENTS.md): operating guidelines for working in the repository.

## Stack

- Angular 20
- ASP.NET Core Web API with .NET 9

## Structure

```text
localscore/
├── frontend/                          # Angular application
├── backend/
│   ├── LocalScore.sln                 # Solution to open in Visual Studio
│   └── src/
│       ├── LocalScore.Api/            # HTTP, application composition, and configuration
│       ├── LocalScore.Application/    # Future use cases
│       ├── LocalScore.Domain/         # Future business rules
│       └── LocalScore.Infrastructure/ # Future persistence and integrations
```

## Prerequisites

- .NET SDK 9.0.203 or a compatible patch release from the 9.0 line
- Node.js 22
- npm 10
- Angular CLI 20

## Running locally

1. Start the API:

   ```powershell
   dotnet run --project backend/src/LocalScore.Api
   ```

2. In another terminal, start the frontend:

   ```powershell
   Set-Location frontend
   npm start
   ```

The frontend runs at `http://localhost:4200`. The API exposes HTTP at `http://localhost:5174` and HTTPS at `https://localhost:7106` through the local launch profiles.

## API checks

- `GET /health`: confirms that the API process is running.
