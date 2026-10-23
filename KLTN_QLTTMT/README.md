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

---

# LHL Art — giao diện website trung tâm mỹ thuật

Frontend React + TypeScript được dựng trước theo nghiệp vụ đồ án. Website hiện đã gọi API thật. Đọc [hướng dẫn nối API và chạy web](NOI_API.md). Các file dữ liệu mẫu được giữ để tham khảo nhưng không được dùng trong ứng dụng đang chạy.

## Chạy dự án

```powershell
cd D:\long2026\KLTN_QLTTMT
npm install
npm run dev
```

Nếu máy chưa cài Node.js/npm, có thể mở `preview.html` qua một web server tĩnh để xem giao diện ngay. Không mở trực tiếp bằng `file://` nếu trình duyệt chặn tài nguyên.

## Cấu trúc chính

- `src/App.tsx`: trang chủ, bộ lọc lớp và luồng đăng ký 3 bước.
- `src/components/MemberArea.tsx`: khu học tập với lịch/điểm danh, nhận xét, học bù, đổi lịch/lớp và phản hồi. Xem [hướng dẫn](docs/member-learning.md).
- `src/components/LearningStudentPicker.tsx`: chọn học viên có lớp đã xác nhận từ nút Học tập.
- `src/components/TeacherCorner.tsx`: trang giáo viên với lịch phân công, điểm danh và nhận xét từng buổi.
- `src/data/mockData.ts`: khóa học, lớp học, đánh giá và ảnh mẫu.
- `src/services/artCenterService.ts`: lớp truy cập dữ liệu, điểm thay thế bằng API sau này.
- `src/types.ts`: kiểu dữ liệu nghiệp vụ dùng chung.
- `src/styles.css`: toàn bộ hệ thống giao diện responsive.
- `src/animations/`: hiệu ứng SVG hoa, lá và cánh hoa bay ngang màn hình.
- `src/animations/`: hiệu ứng SVG hoa, lá và cánh hoa bay ngang màn hình.
- `preview.html`: bản xem trước tĩnh, dùng khi chưa cài được thư viện npm.

## Tích hợp API sau này

Giữ nguyên chữ ký các hàm `getCourses`, `getClasses` và `submitRegistration` trong service. Thay dữ liệu mẫu bằng các lệnh `fetch`/Axios và cấu hình URL qua biến môi trường, ví dụ `VITE_API_BASE_URL`.


## Bản nối API

Đọc [NOI_API.md](NOI_API.md) để biết cách bật API, chạy web, xác minh hồ sơ và cấu hình triển khai. Mã nguồn API tách riêng ở `backend/MyThuat`.
