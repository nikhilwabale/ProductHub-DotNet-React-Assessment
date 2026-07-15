# Final local run guide

## Backend

1. Start SQL Server Express.
2. Open `CRN.ProductAssessment.sln` in Visual Studio.
3. Set `API` as the startup project.
4. Rebuild the solution.
5. Run the API.
6. Confirm Swagger opens and `http://localhost:57071/health` returns a healthy response.

Default local database:

```text
Server=localhost\SQLEXPRESS
Database=CRNProducts
Windows authentication
```

## React UI

Keep the backend running. Open a terminal in `src\Web`:

```bat
npm i
npm run dev
```

Open:

```text
http://localhost:5173
```

The frontend uses a Vite proxy for `/api`, so there is no local CORS or HTTPS-certificate dependency.

## Development credentials

```text
Admin
admin@crn.local
Admin@123
```

```text
Read-only user
user@crn.local
User@123
```

## Assessment coverage

- .NET 8 and ASP.NET Core Web API
- SQL Server and Entity Framework Core
- Product CRUD and related Item operations
- JWT access tokens and refresh-token rotation
- Role-based authorization
- FluentValidation
- Global error middleware
- API versioning
- Service, repository and Unit of Work patterns
- Pagination, async operations, AsNoTracking and response compression
- Swagger/OpenAPI
- Serilog
- xUnit, Moq and WebApplicationFactory tests
- Docker and Docker Compose
- High-level architecture, authentication, setup and deployment documentation
- Additional React UI kept separate from the required backend layers
