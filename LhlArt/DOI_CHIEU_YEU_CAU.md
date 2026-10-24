# Đối chiếu yêu cầu cập nhật ngày 22/10/2026

| Yêu cầu | Kết quả |
|---|---|
| Ghép WinForms với bản nhóm mới | Lấy LhlArt từ e9eb760, ghép lên main 2b8237d; chỉ sửa LhlArt |
| Đăng ký online/offline | Có cột đọc kenhDangKy khi API cung cấp; API hiện chưa lưu nên chưa hoàn tất |
| Viết và tìm tiếng Việt | Ô nhập Unicode/IME, tìm trên giá trị JSON đã giải mã, hỗ trợ có/không dấu, tải đủ trang |
| Thêm/sửa/xóa dữ liệu | Thêm/sửa hồ sơ, đại diện, danh mục, ưu đãi, tài khoản; ngừng sử dụng/gỡ liên kết. Không xóa cứng vì API chưa hỗ trợ |
| Nhận biết tài khoản người học/phụ huynh | Phân loại theo quan hệ đang hiệu lực, có trường hợp vừa học vừa làm phụ huynh và chưa liên kết |
| Họ tên người quan hệ với học viên | Hiện tên hai phía trong danh sách; chọn theo họ tên khi liên kết |
| Ràng buộc năm sinh | Từ 1900 tới hôm nay; tự đại diện đủ 18 tuổi; API kiểm tra tuổi tối thiểu khóa |
| Đăng ký tài khoản riêng | Menu riêng; tạo xong không tự tạo đăng ký lớp |
| Quan hệ bản thân tự điền | Lấy tên, ngày sinh đã có và địa chỉ từ người liên hệ được chọn; liên hệ khẩn cấp nhập riêng |
| WinForms lưu nhưng web chưa thấy | Lưu đúng thành viên–học viên–quan hệ, kiểm tra HTTP hai chiều; web cần tải lại và cùng API |
| Bỏ duyệt tài khoản | Bỏ nút xác minh tài khoản, dùng đăng ký tự hoạt động của API main |
| Tạo mã giảm giá, áp khóa và hiện web | Form ưu đãi tự động hoặc mã riêng, chọn khóa, tỷ lệ/số tiền, hạn mức, thời hạn, kiểm tra công khai |

Không xác nhận hoàn tất các endpoint backend chưa có. Xem YEU_CAU_API.md và KIEM_TRA_BAN_CAP_NHAT.md. Việc đối chiếu này dựa trên yêu cầu trong cuộc trò chuyện và source Git hiện tại, không phải rà soát lại đề cương PDF cũ.
