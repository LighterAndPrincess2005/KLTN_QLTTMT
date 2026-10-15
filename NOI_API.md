# Chạy website và API cùng WinForms

Hướng dẫn chính, dữ liệu và toàn bộ tài khoản đã được thống nhất tại [README.md](README.md).

Dùng `scripts/Start-Center.ps1 -Service Api` để API đọc database `MyThuatCenter`, sau đó `-Service Web` để mở website. WinForms dùng `-Service WinForms`.

Mặc định web gửi `/api` qua Vite tới `http://127.0.0.1:5080`. Có thể đổi đích bằng `VITE_API_PROXY_TARGET` trong `.env`. Khi build triển khai, dùng `VITE_API_BASE_URL` và cấu hình `WebOrigins` trên API, hoặc dùng reverse proxy chung.

API hiện đã có danh sách đăng ký cho nhân viên, các trường chọn học viên, lịch giáo viên, tiến độ/học bù, gửi đề nghị và rút hồ sơ nháp. Các phần này được hợp nhất từ mã nguồn nhóm, không dùng mockData khi chạy.
