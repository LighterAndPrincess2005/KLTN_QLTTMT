# Phạm vi WinForms kết nối API

Đăng nhập bằng tài khoản nhân viên, dùng cùng SQL Server với website. Thông tin đăng nhập mẫu và cách chạy tại [README gốc](../README.md).

| Nhóm | Trạng thái |
|---|---|
| Hồ sơ | Danh sách, xác minh học viên/thành viên, quyền đại diện, đánh giá đầu vào |
| Đào tạo | Thêm/sửa loại khóa, khóa, nội dung buổi, lớp, phòng, giáo viên; xếp lịch |
| Đăng ký | Đọc phiếu từ website, phân trang, công nợ, thu tiền |
| Học tập | Lịch theo quyền, danh sách điểm danh, nhận xét và điểm sản phẩm |
| Hỗ trợ | Đọc/trả lời phản hồi; duyệt/từ chối hủy/chuyển và hoàn tất chuyển theo điều kiện API |
| Kho | Thêm/sửa họa cụ, nhà cung cấp; xem tồn kho và phiếu kho |
| Báo cáo | Tổng quan; biểu đồ học phí/hoàn phí/họa cụ 6 tháng cho tài khoản có quyền |
| Quản trị | Tra cứu tài khoản và nhật ký |

Danh sách tự cập nhật 30 giây khi cửa sổ đang hoạt động; thao tác Làm mới cập nhật ngay. Khi sửa dữ liệu gửi RowVersion để tránh ghi đè thay đổi của người khác. Nhập số tiền theo VND; bảng tiền có đơn vị ₫ và cột số căn phải.

Khóa đã được sử dụng cần tạo phiên bản mới. API từ chối công bố khóa thiếu nội dung, lớp thiếu lịch/giáo viên, ghi điểm danh sai phân công hoặc thu tiền sai điều kiện.

Chưa có đủ giao diện cho phiếu kho nhiều dòng, hoàn tiền, xếp học bù và tạo/đổi quyền/reset tài khoản: dùng Swagger theo đúng vai trò. JSON `--local` là hệ thống cũ độc lập, không đồng bộ; không dùng để xử lý phiếu gửi từ web.

Không có bản EXE build sẵn trong repository. Xem [kiểm tra](../docs/INTEGRATION_CHECKS.md) để biết phần đã xác minh trên môi trường bàn giao.
