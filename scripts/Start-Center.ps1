param(
    [ValidateSet('Api','Web','WinForms','Seed','Check')][string]$Service = 'Api',
    [string]$ConnectionString = 'Server=(localdb)\MSSQLLocalDB;Database=MyThuatCenter;Trusted_Connection=True;TrustServerCertificate=True'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Set-Location $projectRoot
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__MyThuat = $ConnectionString
switch ($Service) {
    'Seed' { dotnet run --project backend/MyThuat/MyThuat.Api -c Release --no-launch-profile -- --seed-center }
    'Api' { dotnet run --project backend/MyThuat/MyThuat.Api -c Release --no-launch-profile -- --urls http://127.0.0.1:5080 }
    'Web' { npm run dev }
    'WinForms' { dotnet run --project LhlArt/LhlArt.csproj -c Release }
    'Check' {
        dotnet run --project backend/MyThuat/MyThuat.Checks -c Release
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        dotnet run --project backend/MyThuat/MyThuat.Checks -c Release -- --center
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        dotnet run --project LhlArt/ConnectionChecks -c Release
    }
}
exit $LASTEXITCODE
