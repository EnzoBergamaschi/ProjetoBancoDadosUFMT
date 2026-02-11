@echo off
echo Building Backend Locally...
cd SimpleCrud.Api
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish
if %ERRORLEVEL% NEQ 0 (
    echo Error building backend.
    exit /b %ERRORLEVEL%
)
cd ..

echo Starting Docker Containers...
docker-compose up -d --build

echo Done! Application should be running at http://localhost
pause
