@echo off
cd /d "%~dp0"
dotnet run --project "backend/MyThuat/MyThuat.Api/MyThuat.Api.csproj" -c Release
pause
