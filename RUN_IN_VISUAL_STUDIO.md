# Run ProductHub in Visual Studio 2022

## Prerequisites

- Visual Studio 2022 with **ASP.NET and web development** workload
- .NET 8 SDK
- SQL Server Express instance named `SQLEXPRESS`
- Node.js 20 or later

## Backend

1. Open `CRN.ProductAssessment.sln`.
2. In Solution Explorer, right-click `API` and select **Set as Startup Project**.
3. At the top toolbar select the `https` profile.
4. Press `F5`.
5. Swagger opens at `https://localhost:57070/swagger`.

The local connection string is:

```text
Server=localhost\SQLEXPRESS;Database=CRNProducts;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

The API automatically creates the `CRNProducts` database and seeds test users/products.

## React UI

Open a terminal in `src/Web`:

```bash
npm i
npm run dev
```

Open `http://localhost:5173`.

## Test credentials

- Admin: `admin@crn.local` / `Admin@123`
- User: `user@crn.local` / `User@123`


## If Visual Studio says API.exe cannot be found

That means the solution did not build. Use **Build > Rebuild Solution** first.
The corrected project removes the invalid `FluentValidation.TestHelper` package, aligns AutoMapper through the Application project, and includes the required `Infrastructure.Data` namespace in the database seeder.

After extracting this corrected ZIP into a new folder:

1. Delete any old `.vs`, `bin`, and `obj` folders if you copied over an older version.
2. Open `CRN.ProductAssessment.sln`.
3. Right-click the solution and choose **Restore NuGet Packages**.
4. Select **Build > Rebuild Solution**.
5. Confirm `0 Error(s)`.
6. Run the `API` startup project with F5.
