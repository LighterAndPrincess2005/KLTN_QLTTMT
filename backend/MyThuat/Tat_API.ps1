$ErrorActionPreference = 'Stop'
$taskLocal = Join-Path $PSScriptRoot 'MyThuat.Api\.local'
$taskTokenPath = Join-Path $taskLocal 'stop-token'
if (Test-Path -LiteralPath $taskTokenPath) {
    try {
        $taskToken = (Get-Content -LiteralPath $taskTokenPath -Raw).Trim()
        Invoke-RestMethod 'http://localhost:5080/local/stop' -Method Post -Headers @{ 'X-Local-Token' = $taskToken } -TimeoutSec 5 | Out-Null
    } catch {
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show('API đã tắt hoặc không đúng phiên đang chạy.', 'API Mỹ Thuật') | Out-Null
    }
}
