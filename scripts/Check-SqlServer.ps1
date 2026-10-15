$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
# Mỗi lệnh tạo database kiểm thử tên ngẫu nhiên và chỉ xóa chính database đó.
dotnet run --project backend/MyThuat/MyThuat.Checks -c Release -- --sqlserver
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet run --project backend/MyThuat/MyThuat.Checks -c Release -- --sqlserver --center
exit $LASTEXITCODE
