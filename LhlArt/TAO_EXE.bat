@echo off
cd /d "%~dp0"
dotnet publish LhlArt.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o BanChay
pause
