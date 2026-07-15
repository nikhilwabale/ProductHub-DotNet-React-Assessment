# Portable local setup

This repository is configured to run on a clean Windows development machine without editing source files.

## Prerequisites

- Visual Studio 2022 with the **ASP.NET and web development** workload
- .NET 8 SDK
- SQL Server LocalDB (normally installed with Visual Studio)
- Node.js 20+ and npm

## Run in Visual Studio

1. Open `CRN.ProductAssessment.sln`.
2. Set `API` as the startup project.
3. Select the `API` launch profile.
4. Rebuild the solution and press `F5`.
5. Swagger opens at `http://localhost:5080/swagger`.
6. Verify `http://localhost:5080/health` before starting React.

The development database is created automatically as:

```text
(localdb)\MSSQLLocalDB / ProductHubAssessmentDb
```

## Run React

Keep the API running, then execute:

```bat
cd src\Web
npm i
npm run dev
```

Open `http://127.0.0.1:5173`.

React proxies `/api` to `http://127.0.0.1:5080`, so there is no HTTPS certificate dependency and no source-code URL change is required.

## Development credentials

```text
Admin: admin@crn.local / Admin@123
User:  user@crn.local  / User@123
```

## Using another SQL Server instance

Override the connection string without modifying committed files:

```bat
set ConnectionStrings__DefaultConnection=Server=.\SQLEXPRESS;Database=ProductHubAssessmentDb;Trusted_Connection=True;TrustServerCertificate=True

dotnet run --project src\API --launch-profile API
```

## Optional Docker execution

```bat
docker compose up --build
```

- UI: `http://localhost:8081`
- Swagger: `http://localhost:8080/swagger`
