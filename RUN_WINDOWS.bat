@echo off
echo Start SQL Server first and update src\API\appsettings.json if needed.
start cmd /k "dotnet run --project src\API"
cd src\Web
call npm install
start cmd /k "npm run dev"
