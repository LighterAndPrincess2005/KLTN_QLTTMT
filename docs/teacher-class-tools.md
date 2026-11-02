# Chính sách và công cụ lớp của giáo viên

Trang `/chinh-sach` liên kết từ FAQ, chân trang, bước xác nhận đăng ký, Góc học tập và Góc giáo viên. Các giới hạn vận hành được đọc từ `/api/noi-dung/quy-dinh`; không tự đặt lại số giờ giữ chỗ hay quyền học bù. Hồ sơ đã lập áp dụng phiên bản quy định đã chốt lúc đăng ký.

## Gửi thông báo lớp

Trong Góc giáo viên, chọn **Gửi thông báo lớp**, chọn lớp, kiểm tra số học viên/liên hệ/email, nhập nội dung và xác nhận gửi. Chỉ giáo viên được phân công ở lớp đang hoạt động mới được gửi. Người nhận là liên hệ đang hoạt động, có quan hệ đại diện đã xác nhận còn hiệu lực, của học viên đã xác nhận đăng ký; một liên hệ nhận một thông báo dù đại diện nhiều học viên.

Thông báo được lưu trên web và đưa từng email vào hàng đợi sẵn có, không công khai địa chỉ cho các người nhận khác. Liên hệ không có email hợp lệ vẫn nhận trên web. Thư không tự thay đổi lịch. UI chỉ báo đã tiếp nhận/đưa vào hàng đợi, không khẳng định đã vào Inbox.

Client tạo khóa cho mỗi nội dung; server lưu ở `ThongBaoLop` và trả kết quả cũ khi gửi lại cùng khóa/nội dung. Gửi nội dung khác với khóa đã dùng bị từ chối. Mỗi giáo viên cần đợi 60 giây trước khi gửi thông báo mới khác cho cùng lớp; retry cùng khóa không bị chặn bởi khoảng đợi này.

## Xếp lịch bù cả lớp

Chọn **Xếp lịch bù cả lớp**, chọn buổi dạy chính chưa bắt đầu, giờ mới và lý do. Giáo viên dùng phòng hiện tại; đổi phòng hoặc xử lý buổi đã qua phải liên hệ học vụ. API kiểm tra quyền phân công chính, trạng thái lớp/buổi, phiên bản lịch, thứ tự nội dung, ngày nghỉ và trùng lịch phòng/lớp/giáo viên/học viên.

Đây là thay thế buổi nghỉ, không phải tạo thêm một buổi hoặc dùng quyền học bù cá nhân. Buổi cũ được hủy, buổi thay thế giữ nội dung và thời lượng, phân công dạy và các điểm danh chờ tham gia được chuyển sang. Kết thúc dự kiến của lớp được cập nhật. Thông báo lịch cũ/mới/phòng/lý do được lưu cùng giao dịch; lỗi kiểm tra không hủy lịch cũ.

Thẻ lịch chỉ hiển thị tên lớp gọn; thứ/giờ theo buổi thực tế để tránh tên lớp cố định gây hiểu nhầm sau khi dời. Lớp chưa có học viên được ghi rõ “Chưa có học viên xác nhận”; không điền giả sĩ số.

## Cập nhật và kiểm thử

API đã có migration thông báo `010_ThongBao.sql`. Sau đó chạy bản API mới với `--upgrade-class-notices` tại thư mục API để thêm `011_ThongBaoLop.sql`. Lệnh tạo bản sao COPY_ONLY và xác minh trước khi thêm bảng, không xóa hoặc sửa hồ sơ hiện có. Database mới tạo bằng EF có bảng tương ứng.

Kiểm thử `LhlArt/IntegrationChecks --sqlserver` sử dụng database tạm và mail giả lập; không gửi email thực tế. Kiểm tra quyền, chống trùng, dữ liệu web, đổi lịch và bảo toàn điểm danh. Kiểm tra giao diện dùng HTTP giả lập cho các thao tác ghi để tránh ảnh hưởng lớp thật.
