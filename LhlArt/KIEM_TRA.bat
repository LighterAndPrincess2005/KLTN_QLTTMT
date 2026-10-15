@echo off
cd /d "%~dp0"
dotnet run --project Tests/SmokeTests.csproj
pause
