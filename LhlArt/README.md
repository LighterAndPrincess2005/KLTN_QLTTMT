# LHL ART — WinForms dùng API

Hướng dẫn chạy và toàn bộ tài khoản mẫu nằm tại [README gốc](../README.md). Mặc định ứng dụng kết nối API, không đọc JSON cục bộ.

```powershell
dotnet run --project LhlArt.csproj -c Release
```

Cấu hình địa chỉ API trong `api.config.json`. Xem [KET_NOI_API.md](KET_NOI_API.md) để biết phạm vi chức năng.

Cấu trúc: Models chứa dữ liệu, Views chứa màn hình, Controllers xử lý biểu mẫu, Networking gọi API. Phân quyền và nghiệp vụ chính được thực thi ở backend. Mã Services/Data cũ chỉ dành cho chế độ `--local`, không nối với website.

Chạy trên Windows 10/11 với .NET 8 SDK. Kiểm tra kết nối: `dotnet run --project ConnectionChecks`. Đóng gói: `dotnet publish LhlArt.csproj -c Release -r win-x64 --self-contained true -o BanChay_API`.
