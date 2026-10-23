@echo off
setlocal
cd /d "%~dp0"
echo Thiet lap 32 tai khoan nhom, mat khau chung Bui@2026.
echo Hay tat API va WinForms truoc khi chay. Database duoc sao luu truoc khi sua.
dotnet run --project "backend/MyThuat/MyThuat.Api/MyThuat.Api.csproj" -c Release -- --setup-group-accounts
if errorlevel 1 goto failed
echo.
echo THANH CONG. Dang nhap WinForms: quantri01 / Bui@2026
echo Chay BAT_API.cmd, sau do BAT_WINFORM.cmd.
pause
exit /b 0
:failed
echo.
echo CHUA HOAN TAT. Gui anh loi phia tren de kiem tra. Khong xoa database.
pause
exit /b 1
