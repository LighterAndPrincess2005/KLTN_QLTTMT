# LHL ART — ứng dụng Windows

Nhánh `winf` chứa mã WinForms tại [`LhlArt`](LhlArt). Nhánh `main` chứa website React. Các file web và backend có sẵn được giữ lại trong lịch sử nhánh; phần WinForms mới nằm riêng trong thư mục này.

## Chạy WinForms

Yêu cầu Windows 10/11 và .NET 8 SDK (hoặc SDK mới hơn có hỗ trợ .NET 8).

1. Khởi động API tại `http://127.0.0.1:5080`.
2. Mở terminal trong thư mục repo và chạy:

```powershell
dotnet run --project LhlArt/LhlArt.csproj
```

Đổi địa chỉ API trong `LhlArt/api.config.json` nếu cần. Đăng nhập bằng tài khoản nhân viên được tạo trên API. Mật khẩu và dữ liệu của máy cá nhân không nằm trong repo.

## Giao diện mới

- Màu kem, xanh và cam san hô đồng bộ với website.
- Đăng nhập có minh họa, ô nhập bo góc và viền khi focus.
- Trang chủ hiển thị số liệu API theo quyền tài khoản.
- Menu có biểu tượng, trạng thái đang chọn; bảng và thanh công cụ dùng chung phong cách.
- Lịch giáo viên dùng `/api/lich-hoc`, tương thích backend hiện có trong repo.

Phạm vi chức năng và các giới hạn API được ghi trong [`KET_NOI_API.md`](LhlArt/KET_NOI_API.md). Đặc biệt backend hiện có chưa hỗ trợ danh sách đăng ký nhân viên; giao diện hiển thị thông báo tại mục đó.

## Kiểm tra

```powershell
dotnet build LhlArt/LhlArt.csproj -c Release
dotnet run --project LhlArt/ConnectionChecks/ConnectionChecks.csproj -c Release
dotnet run --project LhlArt/Tests/SmokeTests.csproj -c Release
```

Bản build, dữ liệu JSON cá nhân, file sao lưu và thông tin đăng nhập không được đưa lên Git.
