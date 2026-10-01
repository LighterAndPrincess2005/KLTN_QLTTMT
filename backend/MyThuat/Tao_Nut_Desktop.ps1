$ErrorActionPreference = 'Stop'
$taskDesktop = [Environment]::GetFolderPath('Desktop')
$taskShell = New-Object -ComObject WScript.Shell
$taskLink = $taskShell.CreateShortcut((Join-Path $taskDesktop 'Bat_API_MyThuat.lnk'))
$taskLink.TargetPath = Join-Path $env:SystemRoot 'System32\wscript.exe'
$taskLink.Arguments = '"' + (Join-Path $PSScriptRoot 'Bat_API_An.vbs') + '"'
$taskLink.WorkingDirectory = $PSScriptRoot
$taskLink.Description = 'Bật API Trung tâm Mỹ thuật và mở trang sử dụng'
$taskLink.WindowStyle = 7
$taskLink.IconLocation = (Join-Path $env:SystemRoot 'System32\shell32.dll') + ',14'
$taskLink.Save()
Write-Output ('Đã tạo nút Desktop: ' + (Join-Path $taskDesktop 'Bat_API_MyThuat.lnk'))
