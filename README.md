# ProductHub — CRN .NET Technical Assessment

A production-style RESTful Product and Item management solution built for the CRN Technosoft .NET Developer assessment.

The assignment remains **backend-first** and preserves the requested projects exactly:

```text
src/API
src/Application
src/Domain
src/Infrastructure
```

A separate React project is included under `src/Web` only as an additional demonstration UI. It consumes the same documented REST API and does not replace any assignment requirement.

## Included requirements

- .NET 8 and ASP.NET Core Web API
- SQL Server with Entity Framework Core
- Product CRUD and related Item CRUD
- Clean layered architecture
- Repository Pattern and Unit of Work
- JWT access token authentication
- Refresh-token rotation and revocation
- Admin/User role authorization
- FluentValidation
- Global consistent error responses
- API versioning under `/api/v1`
- Swagger/OpenAPI with bearer-token support
- Serilog structured logging and HTTP request logging
- Pagination, search, `AsNoTracking()`, indexes, response compression and async APIs
- CORS, HTTPS redirection and security headers
- xUnit, Moq and WebApplicationFactory tests
- Dockerfiles and Docker Compose
- React UI for login, product CRUD, item CRUD, search and pagination

## Architecture

```text
React UI (optional)
        |
        v
ASP.NET Core Controllers
        |
        v
Application Services + DTOs + Validation
        |
        v
Repository Interfaces / Unit of Work
        |
        v
Infrastructure Repositories + EF Core
        |
        v
SQL Server
```

The Domain project contains dependency-free entities, enums, events and exceptions.

## Run with Docker

Create a `.env` file from `.env.example`, then run:

```bash
docker compose up --build
```

- React UI: `http://localhost:8081`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health endpoint: `http://localhost:8080/health`

## Run locally

Requirements: .NET 8 SDK, SQL Server and Node.js 20+.

### Visual Studio (recommended)

1. Open `CRN.ProductAssessment.sln`.
2. Right-click the `API` project and choose **Set as Startup Project**.
3. Select the `https` or `http` launch profile.
4. Press **F5** or **Ctrl+F5**.
5. Swagger opens automatically at `https://localhost:57070/swagger` or `http://localhost:57071/swagger`.

The default local database connection matches the working AMS setup:

```text
Server=localhost\SQLEXPRESS;Database=CRNProducts;Trusted_Connection=True;TrustServerCertificate=True
```

The API creates and seeds the database automatically on first run.

### Command line

```bash
dotnet restore
dotnet run --project src/API --launch-profile http
```

In another terminal:

```bash
cd src/Web
npm i
npm run dev
```

The UI defaults to `http://localhost:5173` and calls the Visual Studio API profile at `http://localhost:57071/api/v1`. Override this using `VITE_API_URL` when needed.

## Development users

- Admin: `admin@crn.local` / `Admin@123`
- User: `user@crn.local` / `User@123`

The Admin can create, update and delete. The User has read-only access. Replace these seeded credentials before production use.

## Main endpoints

```text
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh
POST   /api/v1/auth/revoke
GET    /api/v1/products?pageNumber=1&pageSize=10&search=
GET    /api/v1/products/{id}
POST   /api/v1/products
PUT    /api/v1/products/{id}
DELETE /api/v1/products/{id}
POST   /api/v1/products/{productId}/items
PUT    /api/v1/products/{productId}/items/{itemId}
DELETE /api/v1/products/{productId}/items/{itemId}
```

## Authentication flow

1. Login validates the password hash.
2. The API returns a short-lived JWT and a random refresh token.
3. The client sends `Authorization: Bearer <access-token>`.
4. When the access token expires, the refresh endpoint rotates the refresh token.
5. The old refresh token is revoked and linked to its replacement.
6. Logout/revoke invalidates the current refresh token.

The React UI stores the development session in local storage and automatically attempts one refresh when an API call returns `401`.

## Tests

```bash
dotnet test
```

The solution includes application service tests with Moq, validation tests, repository tests and API integration tests using `WebApplicationFactory`.

## Submission

1. Run the API and React UI locally.
2. Verify Swagger and the main CRUD flow.
3. Run `dotnet test`.
4. Create your own public GitHub repository.
5. Upload the project without secrets or build folders.
6. Capture screenshots of the running React UI and Swagger.
7. Send the repository link and screenshots using the required subject:

```text
CRN Technical Assessment Complete Successfully
```

## Install the React UI

Open a terminal in `src/Web` and use the standard npm commands:

```bash
npm i
npm run dev
```

The project `.npmrc` pins the official npm registry and uses `replace-registry-host=never`, so npm keeps the public URLs from `package-lock.json` instead of replacing them with a machine-level registry.


## Final React UI refinements

- Success and error toast notifications automatically close after three seconds and do not accept clicks.
- The dashboard displays one total-product metric card.
- A single product search field is displayed.
- Product creation and editing use a professional modal dialog.
- The dialog manages product name, description, and total inventory quantity.
- The old inline creation form, expandable item editor, and visible pagination controls were removed from the UI.
- The backend still retains paginated REST endpoints and related Item endpoints as required by the assessment.
- Product update uses `PUT /api/v1/products/{id}` and quantity update uses the related Item route.

## Final React UI refinements

- The browser page itself remains fixed-height on desktop; only the product table scrolls when rows exceed available space.
- The table header remains visible while scrolling.
- Long product descriptions wrap and are limited to three lines in the table, preventing overlap with quantity, date, and actions.
- The View action opens a read-only product details dialog with audit fields and complete description.
- Add and Edit remain modal-based.
- Dashboard spacing is compact so the table receives more screen space.
