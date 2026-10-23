@echo off
cd /d "%~dp0"
dotnet run --project "LhlArt/LhlArt.csproj" -c Release
pause
