@echo off
setlocal
cd /d "%~dp0"

echo ============================================
echo ProductHub local development startup
echo ============================================
echo.
echo Backend: http://localhost:5080/swagger
echo Frontend: http://localhost:5173
echo.

where dotnet >nul 2>nul || (echo ERROR: .NET 8 SDK is not installed.& pause & exit /b 1)
where npm >nul 2>nul || (echo ERROR: Node.js/npm is not installed.& pause & exit /b 1)

start "ProductHub API" cmd /k "cd /d \"%~dp0\" && dotnet run --project src\API --launch-profile API"

echo Waiting for the API to start...
timeout /t 5 /nobreak >nul

pushd src\Web
if not exist node_modules (
  call npm i || (echo ERROR: npm install failed.& pause & exit /b 1)
)
start "ProductHub React UI" cmd /k "npm run dev"
popd

echo.
echo Both applications are starting in separate windows.
echo Open http://localhost:5173 after the API is ready.
pause
