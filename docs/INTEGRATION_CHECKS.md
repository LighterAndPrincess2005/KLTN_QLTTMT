# Kiểm tra bản tích hợp API, website và WinForms

## Kết quả đã thực hiện

| Hạng mục | Kết quả |
|---|---|
| Website | `npm run build`: TypeScript và Vite thành công |
| Backend, WinForms và bộ kiểm tra | Biên dịch C# bằng Roslyn của .NET 8.0.408 thành công, không có cảnh báo ở lần biên dịch cuối |
| Nghiệp vụ API | 153 tình huống HTTP đạt trên database SQLite tạm |
| Bộ dữ liệu và liên thông | 59 kiểm tra đạt trên database SQLite tạm |
| HTTP client WinForms | 9 kiểm tra về token, quyền, RowVersion, lỗi, đăng nhập lại đạt |

Bộ liên thông kiểm tra: đủ dữ liệu mẫu, chạy seed lần hai không tạo trùng, mật khẩu README khớp tài khoản, lịch không trùng phòng, xác nhận đăng ký có đủ học phí, tồn kho không âm theo lô, phân trang, chặn sai vai trò và truy cập hồ sơ người khác. Luồng ghi thực hiện qua HTTP trên database tạm: đăng ký → thu tiền → xác nhận → đọc lịch; gửi phản hồi → nhân viên trả lời; ghi nhận xét → học viên đọc lại. Gửi lại cùng khóa idempotency không tạo phiếu/thu tiền trùng. Biểu đồ lấy số liệu từ chứng từ đã xác nhận.

Không dùng mock HTTP trong 153 và 59 kiểm tra backend. Hai kiểm tra cuối dùng chính `ArtApiClient` của WinForms qua HTTP để đăng nhập giáo viên, đọc 48 buổi học và đăng nhập thu ngân, đọc phiếu đăng ký vừa tạo từ luồng website. Bộ 9 kiểm tra client riêng dùng handler giả để chủ động tạo lỗi và kiểm tra header. Các kiểm tra không sửa database của nhóm.

## Giới hạn kiểm chứng

- Môi trường Linux không chạy được MSBuild vì không đọc được thông tin tiến trình. Đã dùng Roslyn trực tiếp với thư viện tham chiếu .NET/WinForms và dependency NuGet để kiểm tra mã; chưa xác nhận quy trình `dotnet build/publish` trên máy Windows của nhóm.
- Chưa chạy SQL Server/LocalDB ở môi trường bàn giao. SQLite dùng riêng cho kiểm tra; **ứng dụng chính vẫn dùng SQL Server**. SQLite không thay thế kiểm chứng ràng buộc SQL Server, khóa đồng thời hay tải nhiều người dùng.
- Chưa mở cửa sổ WinForms thật; chưa kiểm tra DPI và bố cục trên Windows.
- Chưa kiểm tra web trực quan bằng trình duyệt: môi trường không tải được browser runtime hợp lệ. Vite/TypeScript build và các hợp đồng HTTP đã được kiểm tra.
- Vite còn cảnh báo `use client` của lucide-react; build hoàn thành.

## Chạy lại trên Windows

Từ thư mục gốc repository:

```powershell
npm ci
npm run build
dotnet build LhlArt/LhlArt.csproj -c Release
powershell -ExecutionPolicy Bypass -File scripts/Start-Center.ps1 -Service Check
powershell -ExecutionPolicy Bypass -File scripts/Check-SqlServer.ps1
```

`Check-SqlServer.ps1` tạo database LocalDB kiểm thử riêng tên ngẫu nhiên rồi xóa đúng database đó sau kiểm tra, không dùng `MyThuatCenter` hay `MyThuatDb`.

Để kiểm tra HTTP client WinForms đọc lịch từ API đang bật (chỉ đọc dữ liệu nghiệp vụ):

```powershell
$env:LHL_CHECK_API_URL='http://127.0.0.1:5080'
$env:LHL_CHECK_USER='giaovien01'
$env:LHL_CHECK_PASSWORD='LhlArt2026!HocTap'
dotnet run --project LhlArt/ConnectionChecks -c Release -- --live-schedule
```

Sau đó thực hiện các bước “Kiểm tra đồng bộ” trong README bằng giao diện web và WinForms. Chỉ dùng database mẫu cho thao tác thu tiền giả.
