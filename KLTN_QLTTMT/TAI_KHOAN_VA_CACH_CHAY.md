# Tài khoản đăng nhập theo bộ của nhóm

Mật khẩu chung sau khi thiết lập thành công: **Bui@2026**.

| Tài khoản | Vai trò | Nơi đăng nhập |
|---|---|---|
| quantri01 | Quản trị | WinForms |
| hocvu01 | Học vụ | WinForms |
| thungan01 | Thu ngân | WinForms |
| kho01 | Quản lý kho | WinForms |
| gv01–gv08 | Giáo viên | Theo chức năng giáo viên của ứng dụng |
| hv01–hv20 | Thành viên/học viên | Website |

1. Giải nén vào thư mục mới. Giữ cấu hình kết nối database của nhóm nếu dùng SQL Server riêng.
2. Tắt API và WinForms cũ trước khi chạy.
3. Bấm THIET_LAP_TAI_KHOAN.cmd. Chờ dòng THIET LAP THANH CONG.
4. Bấm BAT_API.cmd, giữ cửa sổ chạy. Bấm BAT_WINFORM.cmd.
5. Đăng nhập quantri01 / Bui@2026.

Cần Windows, .NET SDK và SQL Server/LocalDB như bản gốc. Thiết lập tạo database nếu chưa có,
sao lưu trước khi sửa, tạo quản trị nếu thiếu, bổ sung bộ dữ liệu nền/MAIN nếu chưa có,
và đặt mật khẩu cùng trạng thái hoạt động cho đúng 32 tài khoản trên.
Các tài khoản khác không bị đổi mật khẩu. Không xóa dữ liệu hiện có. Phiên cũ của tài khoản
được đổi bị thu hồi. Không tự đặt lại mật khẩu khi chạy API bình thường.
Nếu dữ liệu cũ không tương thích hoặc có tài khoản trùng sai vai trò, chương trình dừng và
hiển thị lỗi; không xóa database để chạy lại. Sao lưu ở backend/MyThuat/MyThuat.Api/.local/backups.
Nếu một bước thất bại, các bước đã hoàn tất trước đó có thể vẫn được lưu; gửi thông báo lỗi để kiểm tra.

Nếu đăng nhập vẫn lỗi: kiểm tra LhlArt/api.config.json trỏ http://127.0.0.1:5080/ và API đang chạy
phải là bản vừa thiết lập, dùng đúng database. Chỉ giải nén ZIP không thể thay đổi database trên máy bạn.
Mật khẩu trong bảng là mật khẩu được đặt khi bạn chạy lệnh thiết lập; sau khi tự đổi, dùng mật khẩu mới.

Bản sửa này đã bổ sung công cụ thiết lập, không có database SQL Server bên trong ZIP.

## Kiểm tra bản sửa

API build thành công, không lỗi/cảnh báo. Kiểm tra trên database SQLite tạm: nạp MAIN khi đã có quantri01, không tạo trùng quản trị; đặt mật khẩu và mở khóa; thu hồi phiên cũ; giữ tài khoản ngoài phạm vi; chạy lại không đổi tiếp; từ chối vai trò không khớp. Chưa chạy sao lưu SQL Server hoặc giao diện Windows trong môi trường kiểm tra.

Chạy bộ kiểm tra: `dotnet run --project AccountChecks -c Release`. Bộ này chỉ dùng database tạm, không sửa database của nhóm.
