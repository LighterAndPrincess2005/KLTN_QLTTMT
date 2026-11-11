# Mỹ Thuật Bụi — Hệ thống quản lý trung tâm mỹ thuật

**Khóa luận tốt nghiệp · Mã đề tài CNTT-KLCN082**

Hệ thống hỗ trợ trung tâm quản lý khóa học, lớp học, học viên, giáo viên, học phí và họa cụ. Website và ứng dụng quản lý WinForms cùng đọc và cập nhật dữ liệu qua ASP.NET Core Web API và SQL Server.

| Thành phần | Công nghệ | Chức năng chính |
| --- | --- | --- |
| Website (`src/`) | React, TypeScript, Vite | Tra cứu khóa và lớp, đăng ký học, theo dõi học phí và lịch học, gửi tư vấn và đánh giá. Giáo viên xem lịch dạy và điểm danh. |
| Quản lý (`LhlArt/`) | Windows Forms, .NET 8 | Quản lý hồ sơ, đào tạo, đăng ký, đối chiếu học phí, kho họa cụ và báo cáo. |
| API (`backend/MyThuat/MyThuat.Api/`) | ASP.NET Core, Entity Framework Core, SQL Server | Xử lý nghiệp vụ, phân quyền và lưu dữ liệu dùng chung. |

## Chạy trên máy Windows

Cần **.NET 8 SDK** cho WinForms và **.NET SDK 9.0.304** theo `backend/MyThuat/global.json` để chạy các lệnh trong thư mục backend, **SQL Server Express LocalDB** (instance `MSSQLLocalDB`), **Node.js 22.12+ hoặc 24 LTS** và **pnpm**. Lần đầu cần kết nối Internet để tải thư viện NuGet và JavaScript. Giải nén toàn bộ mã nguồn trước khi chạy.

### 1. Tạo cơ sở dữ liệu và bật API

Mở PowerShell ở thư mục gốc dự án. Các lệnh dưới đây dùng một cơ sở dữ liệu riêng cho buổi chấm trong phiên PowerShell này.

```powershell
Set-Location backend/MyThuat/MyThuat.Api
$env:ConnectionStrings__MyThuat = 'Server=(localdb)\MSSQLLocalDB;Database=MyThuatKhoaLuan;Trusted_Connection=True;TrustServerCertificate=True'
dotnet run -c Release --no-launch-profile -- --initialize
dotnet run -c Release --no-launch-profile -- --urls http://127.0.0.1:5080 --environment Development
```

Mở [trang thiết lập API](http://127.0.0.1:5080/) để tạo tài khoản quản trị lần đầu. [Trang kiểm tra API](http://127.0.0.1:5080/health) trả về trạng thái `ok` khi API đã chạy.

### 2. Chạy website và WinForms

Mở PowerShell mới tại thư mục gốc dự án để chạy website.

```powershell
pnpm install --frozen-lockfile
pnpm run dev
```

Mở [website](http://127.0.0.1:5173/). Vite chuyển các yêu cầu `/api` tới cổng 5080.

Mở PowerShell khác tại thư mục gốc để chạy ứng dụng quản lý.

```powershell
dotnet run --project LhlArt/LhlArt.csproj -c Release
```

Có thể mở `LhlArt/CHAY_UNG_DUNG.bat` sau khi đã cài .NET 8 SDK và bật API. WinForms mặc định kết nối `http://127.0.0.1:5080/` theo `LhlArt/api.config.json`.

### 3. Nạp dữ liệu minh họa

Sau khi đã tạo tài khoản quản trị, dừng API bằng **Ctrl+C**. Chạy trong chính cửa sổ PowerShell đã cấu hình database ở bước 1.

```powershell
dotnet run -c Release --no-launch-profile -- --seed-demo
dotnet run -c Release --no-launch-profile -- --urls http://127.0.0.1:5080 --environment Development
```

Tài khoản thành viên mẫu được tạo trong `.local/Tai_khoan_mau.txt` của API. Để thử đăng ký lớp, dùng tài khoản quản trị kích hoạt quy định trung tâm, sau đó đăng nhập web bằng tài khoản thành viên mẫu.

Nếu đề bài yêu cầu tái hiện đúng dữ liệu lúc báo cáo, khôi phục bản sao lưu SQL Server đã chuẩn bị cho buổi chấm và đổi tên database trong chuỗi kết nối ở bước 1. Với bản sao lưu từ phiên bản cũ, xem các lệnh nâng cấp trong [hướng dẫn đồng bộ dữ liệu](HUONG_DAN_DONG_BO_MAIN.md). Ảnh đã tải lên và được tham chiếu bằng `/api/tep/cong-khai/…` nằm trong `.local/public-media/` của API. Khi chuyển bản sao dữ liệu sang máy chấm, gửi kèm những ảnh được phép chia sẻ.

## Kiểm thử

Chạy từ thư mục gốc dự án.

```powershell
pnpm run build
dotnet run --project LhlArt/IntegrationChecks/IntegrationChecks.csproj -c Release -- --sqlserver
dotnet run --project LhlArt/UiChecks/LhlArt.UiChecks.csproj -c Release
```

Kiểm thử tích hợp dùng cơ sở dữ liệu tạm. Các thao tác kiểm thử thanh toán và gửi thư dùng dữ liệu giả lập. Xem [kịch bản trình diễn](docs/kich-ban-demo-de-cuong.md) và [cấu hình SePay](docs/sepay.md) khi cần.

## Thành phần gói nộp

Dùng mã nguồn của commit nộp cùng báo cáo và tài liệu kiểm thử theo thông báo của Khoa. Nếu cần dữ liệu chấm, bổ sung bản sao lưu đã chuẩn bị riêng. Loại khỏi gói các thư mục `.git`, `node_modules`, `bin`, `obj`, `dist`, `work`, các file log, file `.pdb` và cấu hình riêng chứa khóa hoặc mật khẩu.
