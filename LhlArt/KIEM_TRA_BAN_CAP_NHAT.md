# Kiểm tra bản cập nhật WinForms — 22/10/2026

Nền: main `2b8237d`; WinForms gốc: `dong-bo-api-web-winform` `e9eb760`. Backend và website không sửa.

## Đã thực hiện

- Build Release bằng .NET SDK 8.0.408: thành công, 0 lỗi, 0 cảnh báo.
- 30 kiểm tra ConnectionChecks đạt: HTTP client, token, RowVersion, lỗi API; tìm tiếng Việt có/không dấu và dấu tổ hợp; tải đủ 205 hồ sơ qua 3 trang; ngày sinh, đủ 18 tuổi, mật khẩu; phân loại tài khoản và kênh không rõ.
- 17 kiểm tra tích hợp với API thật của main qua HTTP, dùng database SQLite tạm: tạo tài khoản từ client WinForms rồi đăng nhập bằng luồng web; tài khoản không tự đăng ký lớp; thêm/sửa học viên tiếng Việt và người đại diện; phân loại phụ huynh; ưu đãi công khai 10% và voucher riêng 5%; báo giá đúng 85.000 ₫ trên học phí 100.000 ₫; ghi danh, gửi lại không trùng; thu tiền rồi web xác nhận; đăng ký từ web được client WinForms đọc; gỡ liên kết thu hồi quyền xem hồ sơ.
- Kiểm chứng trực tiếp API hiện thiếu endpoint danh sách nhân viên và thiếu trường kênh. Đây là các phần còn phải phối hợp với nhóm API, không tính là chức năng đã hoàn tất.

## Giới hạn

Không chạy cửa sổ WinForms/UniKey trên Windows, chưa kiểm tra bố cục tại DPI thực tế. Không chạy trình duyệt web tương tác; kiểm tra liên thông dùng cùng endpoint HTTP mà web gọi. Không chạy SQL Server tại môi trường bàn giao; SQLite không kiểm chứng đầy đủ ràng buộc và hành vi đồng thời của SQL Server. Chưa sửa hoặc nạp dữ liệu vào database đang dùng của nhóm.

## Kiểm tra trên máy nhóm

Từ thư mục gốc:

```powershell
dotnet build LhlArt/LhlArt.csproj -c Release
dotnet run --project LhlArt/ConnectionChecks -c Release
dotnet run --project LhlArt/IntegrationChecks -c Release -- --sqlserver
```

Sau đó thực hiện luồng kiểm tra trong README. Thử gõ `Võ Huỳnh Gia Huy` và `vo huynh gia huy`, tìm hồ sơ ở trang sau, sửa tên trên WinForms rồi tải lại web; thử tự đại diện chưa đủ 18; tạo ưu đãi tự động 10% và xem trên web. Dùng database kiểm thử cho thu tiền giả.
