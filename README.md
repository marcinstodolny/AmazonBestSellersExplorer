# AmazonBestSellersExplorer

AmazonBestSellersExplorer is a full-stack sample application for browsing Amazon software best sellers, registering and logging in with JWT, and managing a personal list of favorite products.

The project exposes its own .NET API, stores users and favorites in SQL Server, integrates with RapidAPI for bestseller data, and provides an Angular frontend for the main user flow.

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

### Testing
- xUnit
- `WebApplicationFactory`
- SQL Server LocalDB
- Respawn

## Main Features

- Public best sellers view for Amazon `Software` products in `PL`
- User registration with username + password
- User login with JWT token issuance
- Protected favorites flow:
  - add favorite
  - remove favorite
  - list current user's favorites
- Angular frontend integrated with the backend API

## Architecture

### Backend

The backend follows a lightweight Clean Architecture split into:

- `src/Domain`
  Domain entities and value objects such as `User`, `FavoriteProduct`, and `Username`
- `src/Application`
  Use cases, MediatR commands/queries, validation, contracts, and repository abstractions
- `src/Infrastructure`
  EF Core persistence, repository implementations, authentication services, and RapidAPI integration
- `src/API`
  Controllers, JWT/API configuration, middleware, and HTTP-specific concerns

### Frontend

The Angular application is organized into:

- `core`
  auth state, guards, interceptor, API config
- `shared`
  shared frontend models
- `features/auth`
  login and register flow
- `features/bestsellers`
  public best sellers page
- `features/favorites`
  protected favorites flow with shared state based on Signals

## Demo Flow

1. Open the public best sellers page.
2. Register a new account or log in with an existing one.
3. Add a product from the best sellers list to favorites.
4. Open the favorites page to verify the saved product.
5. Remove the product from favorites and confirm the list updates.

## Local Setup

### Prerequisites

- .NET SDK 10
- Node.js + npm
- SQL Server LocalDB
- RapidAPI key for `real-time-amazon-data`

### Backend

From the repository root:

```powershell
dotnet build AmazonBestSellersExplorer.slnx
dotnet run --project src\API\AmazonBestSellersExplorer.API\AmazonBestSellersExplorer.API.csproj --launch-profile https
```

By default the API uses:

- SQL Server LocalDB
- database: `AmazonBestSellersExplorer`
- Swagger: `https://localhost:7233/swagger`

### Database

The default connection string is in:

- [appsettings.json](src/API/AmazonBestSellersExplorer.API/appsettings.json)

Default value:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AmazonBestSellersExplorer;Trusted_Connection=True;MultipleActiveResultSets=True"
}
```

To apply the current schema locally:

```powershell
dotnet ef database update --project src\Infrastructure\AmazonBestSellersExplorer.Infrastructure\AmazonBestSellersExplorer.Infrastructure.csproj --startup-project src\API\AmazonBestSellersExplorer.API\AmazonBestSellersExplorer.API.csproj
```

### RapidAPI

The backend bestseller endpoint requires a valid RapidAPI key.

Configuration section:

```json
"RapidApi": {
  "BaseUrl": "https://real-time-amazon-data.p.rapidapi.com/",
  "ApiKey": "",
  "ApiHost": "real-time-amazon-data.p.rapidapi.com"
}
```

Set `RapidApi:ApiKey` using one of the following:

- local `appsettings.json` override
- environment variable
- user secrets

Example:

```powershell
dotnet user-secrets set "RapidApi:ApiKey" "<YOUR_RAPIDAPI_KEY>" --project src\API\AmazonBestSellersExplorer.API\AmazonBestSellersExplorer.API.csproj
```

### Frontend

Frontend project:

- `src/Web/AmazonBestSellersExplorer.Web`

Install dependencies and run the Angular app:

```powershell
cd src\Web\AmazonBestSellersExplorer.Web
npm install
npm start
```

Notes:

- the frontend uses relative `/api` calls
- Angular dev server proxies `/api` to `https://localhost:7233`
- make sure the backend API is running before using the frontend

## Tests

Integration tests:

```powershell
dotnet test test\Integration\AmazonBestSellersExplorer.IntegrationTests\AmazonBestSellersExplorer.IntegrationTests.csproj
```

Frontend tests:

```powershell
cd src\Web\AmazonBestSellersExplorer.Web
npm test
```

Practical note:

- the frontend test target uses Angular/Karma and requires a local environment able to start a test browser such as Chrome or Chromium

### Integration Test Notes

Integration tests use:

- `WebApplicationFactory`
- SQL Server LocalDB
- a dedicated test database created per test run
- Respawn for database reset between tests

Environment requirement:

- LocalDB must be available on the machine running the tests

## CI

GitHub Actions CI validates the repository automatically for:

- pushes to `development`
- pull requests targeting `development`

Current CI scope:

- backend restore and build
- backend unit tests
- frontend `npm ci`
- frontend production build

Integration tests are intentionally not part of GitHub Actions yet because they currently depend on SQL Server LocalDB. That setup is suitable for local development, but it is not a reliable default for GitHub-hosted Linux runners. They can be added to CI later after moving to a CI-friendly database strategy.

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

## Current Notes

- `GET /api/bestsellers` requires a valid RapidAPI key configured on the backend
- integration tests require SQL Server LocalDB
- favorites endpoints require JWT authentication
- Angular dev mode expects the API to be reachable at `https://localhost:7233`
- audit logs are currently created for successful registration, successful login, and favorite add/remove operations
