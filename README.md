# LocalScore

Responsive web application for managing amateur football championships.

## Documentation

- [Implementation plan](docs/implementation-plan.md): stages, approved decisions, pending items, and completion criteria.
- [Agent instructions](AGENTS.md): operating guidelines for working in the repository.

## Stack

- Angular 20
- ASP.NET Core Web API with .NET 9
- PostgreSQL
- Entity Framework Core with Npgsql
- ASP.NET Core Identity
- JWT authentication with refresh-token rotation

## Structure

```text
localscore/
├── frontend/                          # Angular application
├── backend/
│   ├── LocalScore.sln                 # Solution to open in Visual Studio
│   ├── src/
│   │   ├── LocalScore.Api/            # HTTP, application composition, and configuration
│   │   ├── LocalScore.Application/    # Use cases and application contracts
│   │   ├── LocalScore.Domain/         # Business rules
│   │   └── LocalScore.Infrastructure/ # Identity, persistence, and integrations
│   └── tests/
│       └── LocalScore.Tests/          # Backend unit tests
└── docs/
    └── implementation-plan.md
```

## Prerequisites

- .NET SDK 9.0.203 or a compatible patch release from the 9.0 line
- Entity Framework Core CLI tools 9.x
- Node.js 22
- npm 10
- Angular CLI 20
- PostgreSQL with a local database named `localscore`

Docker is intentionally not part of the current development environment.

## Running locally

1. Trust the ASP.NET Core development HTTPS certificate if needed:

   ```powershell
   dotnet dev-certs https --trust
   ```

2. Start the API:

   ```powershell
   dotnet run --project backend/src/LocalScore.Api
   ```

3. In another terminal, start the frontend:

   ```powershell
   Set-Location frontend
   npm start
   ```

The frontend runs at `http://localhost:4200` and proxies `/api` requests to the local HTTPS API at `https://localhost:7106`. The API also exposes HTTP at `http://localhost:5174` through its local launch profiles.

## Authentication endpoints

```text
POST /api/v1/auth/register
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
GET  /api/v1/auth/me
```

The access token is stored in the browser's `localStorage`. The raw refresh token is only sent through the secure, HttpOnly `LocalScore.RefreshToken` cookie.

## Verification

Backend build and unit tests:

```powershell
dotnet test backend/LocalScore.sln
```

Angular production build and unit tests:

```powershell
Set-Location frontend
npm run build
npm test -- --watch=false --browsers=ChromeHeadless
```

## API checks

- `GET /health`: confirms that the API process is running.
- `GET /openapi/v1.json`: exposes the OpenAPI document in Development.
