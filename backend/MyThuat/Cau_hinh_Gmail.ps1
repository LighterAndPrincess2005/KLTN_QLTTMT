$ErrorActionPreference = 'Stop'
$senderAddress = 'Vohuynhgiahuy1706@gmail.com'
$mailConfigPath = Join-Path $PSScriptRoot 'MyThuat.Api/.local/mail.json'
Write-Host 'CAU HINH GMAIL GUI MA QUEN MAT KHAU'
Write-Host ('Email gui: ' + $senderAddress)
Write-Host 'Bat xac minh 2 buoc, sau do tao Mat khau ung dung tai https://myaccount.google.com/apppasswords'
Write-Host 'Khong nhap mat khau Gmail thong thuong. Khong gui mat khau vao chat.'
$secureAppPassword = Read-Host 'Dan mat khau ung dung 16 ky tu (se duoc che)' -AsSecureString
$secretPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureAppPassword)
try {
    $appSecret = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($secretPointer).Replace(' ', '')
    if ($appSecret.Length -ne 16) { throw 'Mat khau ung dung Gmail can 16 ky tu. Chua luu cau hinh.' }
    $mailConfig = @{ RecoveryMail = @{ Enabled = $true; Username = $senderAddress; AppPassword = $appSecret } }
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($mailConfigPath)) | Out-Null
    [IO.File]::WriteAllText($mailConfigPath, ($mailConfig | ConvertTo-Json -Depth 4), [Text.UTF8Encoding]::new($false))
    Write-Host 'Da luu cau hinh cuc bo trong .local. Khong dua thu muc nay len Git.'
    Write-Host 'API dang chay se doc lai cau hinh. Tao tai khoan thu voi email nhan that de thu Quen mat khau.'
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($secretPointer)
    $appSecret = $null; $mailConfig = $null; $secureAppPassword.Dispose()
}
