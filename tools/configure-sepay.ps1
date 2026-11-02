param([switch]$Disable)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskConfig = Join-Path $taskRoot 'backend/MyThuat/MyThuat.Api/.local/payment.json'
if ($Disable) {
    if (!(Test-Path -LiteralPath $taskConfig)) { throw 'Chưa có cấu hình cục bộ.' }
    $taskValue = Get-Content -LiteralPath $taskConfig -Raw | ConvertFrom-Json
    $taskValue.SePay.Enabled = $false
    $taskValue | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $taskConfig -Encoding UTF8
    Write-Host 'Đã tắt thanh toán SePay tự động.'
    exit
}
$taskUrl = Read-Host 'URL HTTPS công khai của API (không nhập khóa)'
$taskUri = $null
if (![Uri]::TryCreate($taskUrl,[UriKind]::Absolute,[ref]$taskUri) -or $taskUri.Scheme -ne 'https' -or $taskUri.IsLoopback) { throw 'Cần URL HTTPS công khai của API.' }
$taskKey = Read-Host 'API Key đã đặt trong webhook SePay (được che)' -AsSecureString
$taskPlain = [Net.NetworkCredential]::new('', $taskKey).Password
if ($taskPlain.Length -lt 32) { throw 'Khóa cần ít nhất 32 ký tự ngẫu nhiên.' }
$taskValue = @{SePay=@{Enabled=$true;BankCode='ACB';AccountNumber='36266017';AccountName='Lê Hoàng Bảo Long';WebhookKey=$taskPlain}}
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $taskConfig) | Out-Null
$taskValue | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $taskConfig -Encoding UTF8
$taskPlain=$null; $taskValue=$null; $taskKey.Dispose()
Write-Host ('Đã lưu cấu hình cục bộ. Webhook cần trùng URL: ' + $taskUrl.TrimEnd('/') + '/api/thanh-toan/sepay/webhook')
Write-Host 'Không gửi giao dịch thử. Chỉ sẵn sàng nhận khi dashboard đã lưu cùng URL và API Key.'
