@echo off
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Can cai .NET 8 SDK, sau do mo lai VS Code.
  echo https://dotnet.microsoft.com/download/dotnet/8.0
  pause
  exit /b 1
)
dotnet run --project LhlArt.csproj
if errorlevel 1 pause
