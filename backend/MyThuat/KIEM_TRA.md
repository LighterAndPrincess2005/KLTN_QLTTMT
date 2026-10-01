# Kết quả kiểm tra ngày 01/10/2026

- Build Release toàn solution: thành công, 0 warning, 0 error.
- 87 lượt kiểm tra HTTP trên database SQLite tạm: đạt.
- 87 lượt kiểm tra HTTP trên SQL Server LocalDB tạm: đạt, xác nhận trong Checks.result.txt khi chạy bằng tài khoản Windows người dùng.
- API thực đang chạy trên cổng 5080: GET /health trả service MyThuat.Api; Swagger JSON tải được, schema nhập học viên hiển thị; GET /api/tai-khoan/thiet-lap trả trạng thái thiết lập.
- Đã sửa dư dấu backslash trong kết nối LocalDB của bộ kiểm tra và đưa lỗi khởi tạo vào xử lý ngoại lệ.

Bộ kiểm tra bao gồm quyền 401/403, bảo vệ quản trị cuối, chống cập nhật RowVersion cũ, trùng lịch, sức chứa, đăng ký retry, thu từng phần, tiền dư chờ đối chiếu, hoàn có duyệt, chuyển lớp, điểm danh/học bù, phản hồi, nhập/xuất/cấp/trả/hỏng dụng cụ, thu hồi token và kiểm tra không lộ mật khẩu trong nhật ký.

Các lượt kiểm tra là tuần tự; chưa thay thế kiểm thử tải/race đồng thời, kiểm thử giao diện web/WinForms hoặc nghiệm thu toàn bộ quy định trung tâm. Cổng thanh toán, SMTP/xác minh email và triển khai Internet chưa được tích hợp.
