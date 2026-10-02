$ErrorActionPreference = 'Stop'
$taskRoot = $PSScriptRoot
$taskApi = Join-Path $taskRoot 'MyThuat.Api'
$taskLocal = Join-Path $taskApi '.local'
$taskPublished = Join-Path $taskLocal 'publish'
$taskLog = Join-Path $taskLocal 'khoi-dong.log'
$taskUrl = 'http://127.0.0.1:5080'
New-Item -ItemType Directory -Force -Path $taskLocal | Out-Null
$taskMutex = New-Object System.Threading.Mutex($false, 'Local\MyThuatApiLauncherLong2026')
if (-not $taskMutex.WaitOne(0)) { exit }
try {
    Set-Location -LiteralPath $taskRoot
    $taskHealth = $null
    try { $taskHealth = Invoke-RestMethod "$taskUrl/health" -TimeoutSec 2 } catch {}
    if ($taskHealth) {
        $taskPidPath = Join-Path $taskLocal 'api.pid'
        if (-not (Test-Path -LiteralPath $taskPidPath)) { throw 'Cổng 5080 đang có ứng dụng khác. Đóng ứng dụng đó hoặc kiểm tra cấu hình.' }
        $taskExistingPid = [int](Get-Content -LiteralPath $taskPidPath)
        $taskExistingProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $taskExistingPid"
        $taskExpectedDll = Join-Path $taskPublished 'MyThuat.Api.dll'
        if ($taskHealth.service -ne 'MyThuat.Api' -or -not $taskExistingProcess -or -not $taskExistingProcess.CommandLine.Contains($taskExpectedDll)) {
            throw 'Cổng 5080 không thuộc bản API này. Kiểm tra trước khi bật.'
        }
        Start-Process $taskUrl
        exit
    }
    'Đang kiểm tra và khởi động API...' | Set-Content -LiteralPath $taskLog -Encoding UTF8
    $taskDll = Join-Path $taskPublished 'MyThuat.Api.dll'
    if (-not (Test-Path -LiteralPath $taskDll)) {
        & dotnet restore (Join-Path $taskApi 'MyThuat.Api.csproj') --packages (Join-Path $taskRoot '.nuget\packages') 2>&1 | Out-File -LiteralPath $taskLog -Append -Encoding UTF8
        if ($LASTEXITCODE -ne 0) { throw 'Không tải được thư viện NuGet. Xem khoi-dong.log.' }
        & dotnet publish (Join-Path $taskApi 'MyThuat.Api.csproj') -c Release --no-restore -o $taskPublished 2>&1 | Out-File -LiteralPath $taskLog -Append -Encoding UTF8
        if ($LASTEXITCODE -ne 0) { throw 'Không build được API. Xem khoi-dong.log.' }
    }
    $taskConfigFile = Join-Path $taskApi 'appsettings.Local.json'
    $taskConnection = (Get-Content -LiteralPath (Join-Path $taskApi 'appsettings.json') -Raw | ConvertFrom-Json).ConnectionStrings.MyThuat
    if (Test-Path -LiteralPath $taskConfigFile) {
        $taskOverride = (Get-Content -LiteralPath $taskConfigFile -Raw | ConvertFrom-Json).ConnectionStrings.MyThuat
        if ($taskOverride) { $taskConnection = $taskOverride }
    }
    if ($taskConnection -match '\(localdb\)\\([^;]+)') {
        $taskInstance = $Matches[1]
        & SqlLocalDB start $taskInstance 2>&1 | Out-File -LiteralPath $taskLog -Append -Encoding UTF8
        if ($LASTEXITCODE -ne 0) { throw 'Không khởi động được SQL Server LocalDB. Xem khoi-dong.log.' }
    }
    $taskCheckDll = Join-Path $taskRoot 'MyThuat.Checks\bin\Release\net8.0\MyThuat.Checks.dll'
    $taskVerified = Join-Path $taskLocal 'sql-kiem-tra-ok'
    if ((Test-Path -LiteralPath $taskCheckDll) -and -not (Test-Path -LiteralPath $taskVerified)) {
        & dotnet $taskCheckDll --sqlserver 2>&1 | Out-File -LiteralPath $taskLog -Append -Encoding UTF8
        if ($LASTEXITCODE -ne 0) { throw 'Kiểm tra SQL Server chưa đạt. Xem khoi-dong.log; chưa đưa API vào chạy.' }
        (Get-Date).ToString('O') | Set-Content -LiteralPath $taskVerified
    }
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    Push-Location -LiteralPath $taskApi
    try {
        & dotnet $taskDll --initialize 2>&1 | Out-File -LiteralPath $taskLog -Append -Encoding UTF8
        if ($LASTEXITCODE -ne 0) { throw 'Database chưa sẵn sàng. API giữ nguyên dữ liệu cũ; xem khoi-dong.log để xử lý.' }
        $taskOut = Join-Path $taskLocal 'api.log'
        $taskErr = Join-Path $taskLocal 'api-error.log'
        $taskArgs = @('"' + $taskDll + '"', '--urls', 'http://127.0.0.1:5080')
        $taskProcess = Start-Process -FilePath 'dotnet.exe' -ArgumentList $taskArgs -WorkingDirectory $taskApi -WindowStyle Hidden -PassThru -RedirectStandardOutput $taskOut -RedirectStandardError $taskErr
        $taskProcess.Id | Set-Content -LiteralPath (Join-Path $taskLocal 'api.pid')
        $taskReady = $false
        for ($taskAttempt = 0; $taskAttempt -lt 60; $taskAttempt++) {
            try {
                $taskHealth = Invoke-RestMethod "$taskUrl/health" -TimeoutSec 1
                if ($taskHealth.service -eq 'MyThuat.Api') { $taskReady = $true; break }
            } catch {}
            if ($taskProcess.HasExited) { break }
            Start-Sleep -Milliseconds 500
            $taskProcess.Refresh()
        }
        if (-not $taskReady) { throw 'API chưa bật được. Xem api-error.log và api.log.' }
    } finally { Pop-Location }
    Start-Process $taskUrl
} catch {
    $_.Exception.Message | Add-Content -LiteralPath $taskLog -Encoding UTF8
    Add-Type -AssemblyName System.Windows.Forms
    [System.Windows.Forms.MessageBox]::Show(($_.Exception.Message + [Environment]::NewLine + 'Nhật ký: ' + $taskLog), 'API Mỹ Thuật') | Out-Null
} finally {
    $taskMutex.ReleaseMutex()
    $taskMutex.Dispose()
}
