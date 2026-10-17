param([ValidateRange(1024,65535)][int]$Port = 5080)
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'MyThuat.Api.csproj'
if (-not (Test-Path -LiteralPath $project)) { throw "Khong tim thay project API: $project" }
Write-Host "API dung chung cho cac may nhan vien. Can giu may chu hoat dong."
Get-NetIPConfiguration | Where-Object { $_.IPv4DefaultGateway } | ForEach-Object {
    foreach ($ip in $_.IPv4Address.IPAddress) { Write-Host "Dia chi nhap tren WinForms: http://${ip}:$Port/" }
}
Write-Host "Kiem tra tren may nhan vien: http://<IP-may-chu>:$Port/health"
Write-Host "Firewall: cho phep TCP $Port trong mang Private, pham vi LocalSubnet."
dotnet run --project $project --launch-profile MyThuat.Api.LAN --urls "http://0.0.0.0:$Port"
exit $LASTEXITCODE
