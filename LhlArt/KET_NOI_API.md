# Kết nối WinForms với API mới

Bản này ghép với `main` tại `2b8237d`. Mở [README](README.md) để chạy, cấu hình API và kiểm tra thao tác qua lại. Bỏ luồng duyệt tài khoản cũ; tài khoản tạo bằng API mới hoạt động ngay.

Mọi ghi dữ liệu đều dùng API. Sau khi lưu trên WinForms, tải lại phần tương ứng trên web. Web và WinForms phải trỏ cùng API/database. Dữ liệu JSON cũ không được dùng trong luồng khởi động.

API hiện chưa có danh sách tổng đăng ký cho nhân viên và chưa lưu kênh online/offline. WinForms có tra cứu theo ID, không suy đoán kênh. Chi tiết phối hợp: [YEU_CAU_API.md](YEU_CAU_API.md).
