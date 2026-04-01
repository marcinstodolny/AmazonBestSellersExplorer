# AmazonBestSellersExplorer

AmazonBestSellersExplorer is a full-stack recruitment assignment built around one main user journey: browse Amazon software bestsellers, create an account, sign in, and manage a personal favorites list.

The solution combines a .NET 10 backend, an Angular 20 frontend, SQL Server persistence, and a RapidAPI integration used to fetch bestseller data.

## Project Overview

The application provides:

- a public bestseller catalog for Amazon `Software` products in `PL`
- user registration and login with JWT-based authentication
- a protected favorites flow for authenticated users
- an Angular frontend consuming the application's own .NET API
- RapidAPI integration for bestseller data

## Tech Stack

### Backend

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server / LocalDB
- MediatR
- FluentValidation
- JWT authentication

### Frontend

- Angular 20
- Standalone Components
- PrimeNG `DataView`
- Signals
- Functional HTTP interceptor for JWT

### Testing and Quality

- xUnit
- `WebApplicationFactory`
- SQL Server LocalDB
- Respawn
- GitHub Actions CI

## Key Features

- Public software bestsellers page
- Register and login flow
- JWT token issuance and authenticated API access
- Protected favorites page
- Add/remove favorites directly from the bestseller list
- Persistent favorites stored in SQL Server
- Graceful bestseller-service error handling when RapidAPI is unavailable or misconfigured

## Assignment Coverage

This repository covers the core task requirements:

- public bestsellers flow
- registration and login
- JWT-based authentication
- protected favorites flow
- PrimeNG `DataView`
- Angular standalone components
- functional HTTP interceptor for JWT
- Signals-based state handling where used
- backend validation and consistent error handling
- auditability of key user operations

## Beyond the Original Scope

The repository also includes deliberate quality improvements beyond the base assignment:

- GitHub Actions CI for backend and frontend validation
- backend unit tests
- backend integration tests
- focused unit test for the RapidAPI bestseller request
- global exception handling middleware
- request logging middleware for API requests
- persistent audit logs for key business operations
- frontend polish for loading, error, and favorites interaction states

## Recent Improvements

Recent work added or refined:

- global exception handling middleware for API failures
- request logging middleware with method, path, status code, elapsed time, and user context when available
- persistent audit logs for:
  - successful registration
  - successful login
  - add favorite
  - remove favorite
- improved authenticated bestseller startup flow so favorites are loaded first and heart states stay consistent after login
- graceful handling of missing RapidAPI configuration without failing whole application startup
- dedicated frontend error state for an unavailable or misconfigured bestseller service
- UI and copy polish to make the app feel closer to a finished submission than a scaffold

## Architecture

### Backend

The backend follows a lightweight Clean Architecture split:

- `src/Domain`
  domain entities, rules, and value objects
- `src/Application`
  use cases, validation, contracts, and abstractions
- `src/Infrastructure`
  EF Core persistence, repositories, authentication services, and RapidAPI integration
- `src/API`
  controllers, middleware, and HTTP-specific configuration

### Frontend

The Angular application is organized into:

- `core`
  auth state, guards, interceptor, API config
- `shared`
  shared frontend models
- `features/auth`
  login and register flow
- `features/bestsellers`
  public bestsellers page
- `features/favorites`
  protected favorites flow with shared state based on Signals

## Local Setup

### Prerequisites

- .NET SDK 10
- Node.js + npm
- SQL Server LocalDB
- RapidAPI key for `real-time-amazon-data` if you want the bestseller flow to work against the real external service

### Backend

From the repository root:

```powershell
dotnet build AmazonBestSellersExplorer.slnx
dotnet run --project src\API\AmazonBestSellersExplorer.API\AmazonBestSellersExplorer.API.csproj --launch-profile https
```

Default local API URL:

- `https://localhost:7233`

Swagger:

- `https://localhost:7233/swagger`

In `Development`, the API applies pending EF Core migrations automatically on startup.

### Frontend

Frontend project:

- `src/Web/AmazonBestSellersExplorer.Web`

Run locally:

```powershell
cd src\Web\AmazonBestSellersExplorer.Web
npm install
npm start
```

Notes:

- the frontend uses relative `/api` calls
- Angular dev server proxies `/api` to `https://localhost:7233`
- start the backend first for a usable local flow

## Configuration

### Database

The default connection string is defined in:

- `src/API/AmazonBestSellersExplorer.API/appsettings.json`

Default local value:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AmazonBestSellersExplorer;Trusted_Connection=True;MultipleActiveResultSets=True"
}
```

In normal local development, you do not need to run migrations manually because the API applies pending migrations on startup.

If you want to apply the current schema manually, you can still run:

```powershell
dotnet ef database update --project src\Infrastructure\AmazonBestSellersExplorer.Infrastructure\AmazonBestSellersExplorer.Infrastructure.csproj --startup-project src\API\AmazonBestSellersExplorer.API\AmazonBestSellersExplorer.API.csproj
```

### RapidAPI

RapidAPI is required only for the bestseller integration.

Configuration section:

```json
"RapidApi": {
  "BaseUrl": "https://real-time-amazon-data.p.rapidapi.com/",
  "ApiKey": "",
  "ApiHost": "real-time-amazon-data.p.rapidapi.com"
}
```

Set `RapidApi:ApiKey` for local use, for example with user secrets:

```powershell
dotnet user-secrets set "RapidApi:ApiKey" "<YOUR_RAPIDAPI_KEY>" --project src\API\AmazonBestSellersExplorer.API\AmazonBestSellersExplorer.API.csproj
```

If RapidAPI configuration is missing, the application still starts. Only the bestseller flow becomes unavailable and the frontend shows a dedicated user-friendly error state instead of crashing the entire app.

### JWT

The repository contains development-safe defaults for local startup. In particular, `Jwt:SecretKey` in `appsettings.json` is a local development placeholder kept in the repository so the API can start easily after clone.

For non-local or real use, override `Jwt:SecretKey` outside the repository, for example via user secrets or environment variables.

Example:

```powershell
dotnet user-secrets set "Jwt:SecretKey" "<YOUR_DEVELOPMENT_SECRET_KEY>" --project src\API\AmazonBestSellersExplorer.API\AmazonBestSellersExplorer.API.csproj
```

## Tests

### Backend Unit Tests

```powershell
dotnet test test\Unit\AmazonBestSellersExplorer.UnitTests\AmazonBestSellersExplorer.UnitTests.csproj
```

### Backend Integration Tests

```powershell
dotnet test test\Integration\AmazonBestSellersExplorer.IntegrationTests\AmazonBestSellersExplorer.IntegrationTests.csproj
```

Integration tests use:

- `WebApplicationFactory`
- SQL Server LocalDB
- a dedicated test database
- Respawn for reset between tests

Environment note:

- LocalDB must be available on the machine running the integration tests

### Frontend Tests

```powershell
cd src\Web\AmazonBestSellersExplorer.Web
npm test
```

Practical note:

- frontend tests use Angular/Karma and require a local environment able to launch a browser such as Chrome or Chromium

## CI

GitHub Actions CI runs automatically on:

- pushes to `development`
- pull requests targeting `development`

Current CI scope:

- backend restore and build
- backend unit tests
- frontend `npm ci`
- frontend production build

This CI setup is intentional and goes beyond the original assignment.

Integration tests are currently not executed in GitHub Actions because they depend on SQL Server LocalDB, which is suitable for local development but not a reliable default for GitHub-hosted Linux runners. They can be added later after moving to a CI-friendly database strategy.

## API Endpoints

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`

### Favorites

- `GET /api/favorites`
- `POST /api/favorites`
- `DELETE /api/favorites/{amazonProductId}`

### Bestsellers

- `GET /api/bestsellers`

## Practical Notes / Limitations

- the bestseller flow depends on valid RapidAPI configuration
- missing RapidAPI configuration no longer blocks application startup; it is handled as a dedicated bestseller-service error
- favorites endpoints require JWT authentication
- integration tests require SQL Server LocalDB
- GitHub Actions currently does not run LocalDB-based integration tests


## Screenshots

Example screens are available in `docs/screenshots`.

- `01-bestsellers-guest.png`
- `02-register.png`
- `03-login.png`
- `04-favorites.png`
- `05-bestsellers.png`