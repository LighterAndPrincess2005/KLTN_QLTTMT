@echo off
cd /d "%~dp0"
echo Nap du lieu mau khoa hoc va lop hoc tu tai lieu Word.
echo Can tao tai khoan quan tri tren API truoc khi nap.
dotnet run --project MyThuat.Api -c Release -- --seed-demo
if errorlevel 1 (
  echo Nap that bai. Xem thong bao ben tren.
) else (
  echo Da nap hoac bo mau da ton tai. Khong them trung.
)
pause
