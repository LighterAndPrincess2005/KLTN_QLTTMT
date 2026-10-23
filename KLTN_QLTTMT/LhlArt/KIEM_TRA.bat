@echo off
cd /d "%~dp0.."
dotnet build LhlArt/LhlArt.csproj -c Release
if errorlevel 1 goto end
dotnet run --project LhlArt/ConnectionChecks -c Release
if errorlevel 1 goto end
dotnet run --project LhlArt/IntegrationChecks -c Release
:end
pause
