# Run in Visual Studio

## Requirements

- Visual Studio 2022 with ASP.NET and web development workload
- .NET 8 SDK
- SQL Server LocalDB
- Node.js 20+

## Backend

1. Open `CRN.ProductAssessment.sln`.
2. Set `API` as the startup project.
3. Select the `API` profile.
4. Rebuild and press `F5`.
5. Open `http://localhost:5080/swagger`.
6. Confirm `http://localhost:5080/health` works.

The API automatically creates `ProductHubAssessmentDb` in `(localdb)\MSSQLLocalDB` and seeds development users.

## React UI

Keep the API running, then:

```bat
cd src\Web
npm i
npm run dev
```

Open `http://127.0.0.1:5173`.
